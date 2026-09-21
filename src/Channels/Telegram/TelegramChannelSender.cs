using System.Text.Json;

namespace BotSaaS.Api.Channels.Telegram;

// Outbound proactive message sender via Telegram Bot API using Typed HttpClient.
public class TelegramChannelSender : IChannelSender
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public TelegramChannelSender(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SendMessageAsync(string contactIdentifier, string text, CancellationToken stoppingToken = default)
    {
        string? botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(botToken)) return false;

        // In Telegram, the contact identifier is the numeric chat_id
        if (!long.TryParse(contactIdentifier, out long chatId)) return false;

        string url = $"https://api.telegram.org/bot{botToken}/sendMessage";
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, new TgSendMessage(chatId, text), _jsonOptions, stoppingToken);
        return response.IsSuccessStatusCode;
    }
}
