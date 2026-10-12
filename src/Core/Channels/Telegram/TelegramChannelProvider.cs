using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Channels.Telegram;

public class TelegramChannelProvider : IChannelProvider
{
    private readonly HttpClient _httpClient;
    public TelegramChannelProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public MessageChannel Type => MessageChannel.Telegram;
    public async Task<Result<bool>> ValidateCredentials(string identifier)
    {
        string urlValidate = $"https://api.telegram.org/bot{identifier}/getMe";
        try
        {
            // post to validate by /getme
            HttpResponseMessage response = await _httpClient.PostAsync(urlValidate, null);
            if (!response.IsSuccessStatusCode)
            {
                return Result<bool>.Failure("Invalid telegram bot token");
            }
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to connect to Telegram API {ex.Message}");
        }
    }

    public async Task<Result<bool>> RegisterWebhook(Guid channelId, string identifier, string appBaseUrl)
    {
        string urlTelegram = $"https://api.telegram.org/bot{identifier}/setWebhook";
        string trimUrl = appBaseUrl.TrimEnd('/');
        string webhookUrl = $"{trimUrl}/api/webhooks/telegram/{channelId}";
        
        object payload = new { url = webhookUrl, allowed_updates = new[] {"message"} }; // only messages, no emojis...

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(urlTelegram, payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(); // error message
                return Result<bool>.Failure($"Telegram webhook registration failed: {errorBody}");
            }
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to communicate with Telegram API: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveWebhook(string identifier)
    {
        string urlTelegram = $"https://api.telegram.org/bot{identifier}/deleteWebhook";

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsync(urlTelegram, null);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(); 
                return Result<bool>.Failure($"Telegram webhook removal failed: {errorBody}");
            }
            return Result<bool>.Success(true);
        }
        catch(Exception ex)
        {
            return Result<bool>.Failure($"Failed to communicate with Telegram API: {ex.Message}");
        }

    }
}