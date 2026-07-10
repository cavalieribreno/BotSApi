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

    // Sends the system prompt + conversation history to Gemini and returns the model's reply.
    public async Task<string> GenerateReplyAsync(string systemPrompt, IReadOnlyList<ChatMessage> history)
    {
        // Map our history to Gemini's "contents" format. Gemini names the assistant "model", not "assistant".
        List<object> contents = new List<object>();

        foreach(ChatMessage message in history)
        {
            string role;
            if(message.Role == ChatRole.User)
            {
                role = "user";
            }
            else
            {
                role = "model";
            }
            contents.Add(new
            {
                role = role,
                parts = new[]
                {
                    new { text = message.Content }
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
        if (response.IsSuccessStatusCode)
        {
            GeminiResponse? responseBody = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            string textoGemini = responseBody!.Candidates[0].Content.Parts[0].Text; // object
            return textoGemini;
        }
        return "Erro";
    }
}   