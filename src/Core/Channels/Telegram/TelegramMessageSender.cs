using System.Text.Json;

namespace BotSaaS.Api.Core.Channels.Telegram;

// Outbound proactive message sender via Telegram Bot API using Typed HttpClient.
public class TelegramMessageSender : IMessageSender
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public TelegramMessageSender(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SendMessageAsync(string contactIdentifier, string text, string? channelIdentifier = null, CancellationToken stoppingToken = default)
    {
        if (string.IsNullOrWhiteSpace(channelIdentifier))
        {
            return false;
        }

        // In Telegram, the contact identifier is the numeric chat_id
        if (!long.TryParse(contactIdentifier, out long chatId))
        {
            return false;
        }

        string url = $"https://api.telegram.org/bot{channelIdentifier}/sendMessage";
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, new TgSendMessage(chatId, text), _jsonOptions, stoppingToken);
        return response.IsSuccessStatusCode;
    }
}
