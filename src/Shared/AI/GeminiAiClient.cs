namespace BotSaaS.Api.Shared.AI;

// Gemini implementation of IAiClient: builds the request from the system prompt + history, calls the API, returns the reply text.
public class GeminiAiClient : IAiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelAi;

    public GeminiAiClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
            ?? throw new InvalidOperationException("GEMINI_API_KEY não definido");
        _modelAi = Environment.GetEnvironmentVariable("GEMINI_MODEL")
            ?? throw new InvalidOperationException("GEMINI_MODEL não definido");            
    }

    // Sends the system prompt + conversation history to Gemini and returns the reply as text.
    // TODO: 'tools' is ignored -- Gemini tool calling not implemented yet. Switching DI to Gemini disables action extraction.
    public async Task<AiResponse> GenerateReplyAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, IReadOnlyList<ToolDefinition> tools)
    {
        // Map our history to Gemini's "contents" format. Gemini names the assistant "model", not "assistant".
        List<object> contents = new List<object>();

        foreach(ChatMessage message in history)
        {
            // Only text-carrying forms map to Gemini. Tool call/result never occur here: this client ignores `tools`
            // (see TODO above), so the model never emits a ToolCallReply and the loop never appends those forms.
            string role;
            string text;
            switch (message)
            {
                case UserMessage u: role = "user"; text = u.Content; break;
                case AssistantMessage a: role = "model"; text = a.Content; break;
                default: continue;
            }
            contents.Add(new
            {
                role = role,
                parts = new[]
                {
                    new { text = text }
                }
            });
        }
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelAi}:generateContent?key={_apiKey}";

        // system_instruction (the bot's role) is a separate field, outside "contents".
        var requestBody = new
        {
            contents = contents,
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = systemPrompt }
                }
            }
        };
        
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody);
        if (!response.IsSuccessStatusCode)
        {
            throw new AiClientException($"Gemini retornou status: {(int)response.StatusCode}");
        }
        GeminiResponse? responseBody = await response.Content.ReadFromJsonAsync<GeminiResponse>();
        if(responseBody is null)
        {
            throw new AiClientException($"Gemini retornou null");
        }
        string textoGemini = responseBody.Candidates[0].Content.Parts[0].Text; // object
        return new TextReply(textoGemini);
    }
}   