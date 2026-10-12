using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Channels;

// neutral provider for channels connections (Telegram, WhatsApp...)
public interface IChannelProvider
{
    MessageChannel Type { get; }
    Task<Result<bool>> ValidateCredentials(string identifier);
    Task<Result<bool>> RegisterWebhook(Guid channelId, string identifier, string appBaseUrl);
    Task<Result<bool>> RemoveWebhook(string identifier);
}