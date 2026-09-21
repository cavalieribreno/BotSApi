namespace BotSaaS.Api.Channels;

// Neutral contract for proactive message sending across channels (Telegram, WhatsApp).
public interface IChannelSender
{
    Task<bool> SendMessageAsync(string contactIdentifier, string text, CancellationToken stoppingToken = default);
}
