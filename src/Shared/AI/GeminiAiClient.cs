namespace BotSaaS.Api.Shared.AI;

public class GeminiAiClient
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

    public async Task<string> GenerateContentAsync(string prompt)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelAi}:generateContent?key={_apiKey}";

        var requestBody = new
        {
            contents = new []
            {
                new
                {
                    parts = new []
                    {
                        new { text = prompt }
                    }   
                }
            },
            generationConfig = new
            {
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