namespace BotSaaS.Api.Core.Channels;

// Neutral contract for proactive message sending across channels (Telegram, WhatsApp).
public interface IMessageSender
{
    Task<bool> SendMessageAsync(string contactIdentifier, string text, string? channelIdentifier = null, CancellationToken stoppingToken = default);
}
