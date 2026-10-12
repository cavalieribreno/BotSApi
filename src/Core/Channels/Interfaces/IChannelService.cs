using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Channels;

public interface IChannelService
{
    Task<List<Channel>> GetChannelsByCompany(Guid companyId);
    Task<Result<ChannelResponse>> ConnectChannel(Guid companyId, MessageChannel type, string identifier);
    Task<Result<bool>> DisconnectChannel(Guid companyId, Guid channelId);
}