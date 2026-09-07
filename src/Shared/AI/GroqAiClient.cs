using System.Net.Http.Headers;

namespace BotSaaS.Api.Shared.AI;

// Groq implementation of IAiClient (OpenAI-format API): system prompt goes as the first "system" message, assistant role is "assistant".
public class GroqAiClient : IAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelGroq;
    
    public GroqAiClient(HttpClient httpClient)
    {
        _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")
            ?? throw new InvalidOperationException("GROQ_API_KEY não definido");
        _modelGroq = Environment.GetEnvironmentVariable("GROQ_MODEL")
            ?? throw new InvalidOperationException("GROQ_MODEL não definido");
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }
    // Sends the system prompt + conversation history to Groq and returns the model's reply or tool call.
    public async Task<AiResponse> GenerateReplyAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, IReadOnlyList<ToolDefinition> tools)
    {
        List<object> contents = new List<object>();
        contents.Add( new { role = "system", content = systemPrompt});

        foreach(ChatMessage message in history)
        {
            switch (message)
            {
                case UserMessage u:
                    contents.Add( new { role = "user", content = u.Content });
                    break;

                case AssistantMessage a:
                    contents.Add( new { role = "assistant", content = a.Content });
                    break;
                
                case AssistantToolCall atc:
                    contents.Add( new 
                    {   
                        role = "assistant",
                        content = (string?) null,
                        tool_calls = new[]
                        {
                            new
                            {
                                id = atc.Id,
                                type = "function",
                                function = new { name = atc.Name, arguments = atc.ArgumentsJson }
                            }
                        } 
                    });
                    break;
                
                case ToolResult tr:
                    contents.Add( new { role = "tool", tool_call_id = tr.ToolCallId, content = tr.Content });
                    break;
            }
        }
        string url = $"https://api.groq.com/openai/v1/chat/completions";

        // Translate the neutral tool definitions into Groq's (OpenAI) tools format.
        List<object> toolsPayload = new List<object>();
        foreach (ToolDefinition tool in tools)
        {
            // Dictionary (not anonymous object): "properties" keys are the param names, known only at runtime -> serializes to a JSON object keyed by name.
            Dictionary<string, object> properties = new Dictionary<string, object>();
            List<string> required = new List<string>();
            foreach (ToolParameter parameter in tool.Parameters)
            {
                properties[parameter.Name] = new { type = parameter.Type, description = parameter.Description };
                if (parameter.Required)
                {
                    required.Add(parameter.Name);
                }
            }
            toolsPayload.Add(new
            {
                type = "function",
                function = new
                {
                    name = tool.Name,
                    description = tool.Description,
                    parameters = new
                    {
                        type = "object",
                        properties = properties,
                        required = required
                    }
                }
            });
        }
        var requestBody = new
        {
            model = _modelGroq,
            messages = contents,
            tools = toolsPayload
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody);
        if (!response.IsSuccessStatusCode)
        {
            throw new AiClientException($"Groq retornou: {(int)response.StatusCode}");
        }
        GroqResponse? responseBody = await response.Content.ReadFromJsonAsync<GroqResponse>();
        if(responseBody is null)
        {
            throw new AiClientException($"Groq retornou null");
        }

        // model asked to call a tool - surface it neutrally (Core decides what to do)
        GroqMessage groqMessage = responseBody.Choices[0].Message;
        if(groqMessage.ToolCalls is not null && groqMessage.ToolCalls.Count > 0)
        {
            GroqToolCall groqToolCall = groqMessage.ToolCalls[0];
            return new ToolCallReply(groqToolCall.Id, groqToolCall.Function.Name, groqToolCall.Function.Arguments);
        }
        // plain text reply
        else
        {
            return new TextReply(groqMessage.Content ?? "");
        }
    }
}