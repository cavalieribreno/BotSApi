using BotSaaS.Api.Core.Conversations;

namespace BotSaaS.Api.Core.Channels;

public interface IChannelRepository
{
    Task InsertChannel(Channel channel);
    Task<List<Channel>> GetChannelsByCompany(Guid companyId);
    // Scoped tenant lookup: enforces isolation to prevent cross-tenant access (IDOR defense).
    Task<Channel?> GetChannelByIdAndCompany(Guid id, Guid companyId);

    // Public webhook tenant resolution: resolves tenant ownership from inbound route {channelId}.
    Task<Channel?> GetActiveChannelById(Guid id);

    Task<Channel?> GetActiveChannelByIdentifier(MessageChannel type, string identifier);
    Task<int> DeactivateChannel(Guid id, Guid companyId);
}