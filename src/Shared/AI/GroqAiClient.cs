using System.Net.Http.Headers;

namespace BotSaaS.Api.Shared.AI;

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
    public async Task<string> GenerateReplyAsync(string systemPrompt, IReadOnlyList<ChatMessage> history)
    {
        List<object> contents = new List<object>();
        contents.Add( new { role = "system", content = systemPrompt});

        foreach(ChatMessage message in history)
        {
            string role;
            if(message.Role == ChatRole.User)
            {
                role = "user";
            }
            else
            {
                role = "assistant";
            }
            contents.Add( new
            {
                role = role,
                content = message.Content
            });
        }
        string url = $"https://api.groq.com/openai/v1/chat/completions";

        var requestBody = new
        {
            model = _modelGroq,
            messages = contents
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody);
        if (response.IsSuccessStatusCode)
        {
            GroqResponse? responseBody = await response.Content.ReadFromJsonAsync<GroqResponse>();
            string textoGroq = responseBody!.Choices[0].Message.Content;
            return textoGroq;
        }
        return "Erro";
    }
}