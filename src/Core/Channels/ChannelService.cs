using System.Data.Common;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Channels;

public class ChannelService : IChannelService
{
    private readonly IEnumerable<IChannelProvider> _channelProviders;
    private readonly IChannelRepository _channelRepository;
    private readonly DbSession _dbSession;
    private readonly IDatabase _databaseConnection;
    public ChannelService(IEnumerable<IChannelProvider> channelProviders, IChannelRepository channelRepository, DbSession dbSession, IDatabase databaseConnection)
    {
        _channelProviders = channelProviders;
        _channelRepository = channelRepository;
        _dbSession = dbSession;
        _databaseConnection = databaseConnection;
    }
    public async Task<List<Channel>> GetChannelsByCompany(Guid companyId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        return await _channelRepository.GetChannelsByCompany(companyId);
    }

    public async Task<Result<ChannelResponse>> ConnectChannel(Guid companyId, MessageChannel type, string identifier)
    {
        IChannelProvider? providerConnect = null;
        foreach(IChannelProvider provider in _channelProviders)
        {
            if(provider.Type == type)
            {
                providerConnect = provider;
                break; // found the provider, stop
            }
        }
        if(providerConnect == null)
        {
            return Result<ChannelResponse>.Failure($"Channel type '{type}' is not supported.");
        }

        Result<bool> credentialValidation = await providerConnect.ValidateCredentials(identifier);
        if (!credentialValidation.IsSuccess)
        {
            return Result<ChannelResponse>.Failure(credentialValidation.Error ?? "Invalid channel credentials.");
        }

        Channel channel = new Channel
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Type = type,
            Identifier = identifier,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync(); 

        await _channelRepository.InsertChannel(channel);

        string? appBaseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL");
        if (string.IsNullOrWhiteSpace(appBaseUrl))
        {
            return Result<ChannelResponse>.Failure("Environment variable APP_BASE_URL is not configured.");
        }

        Result<bool> webhookResult = await providerConnect.RegisterWebhook(channel.Id, identifier, appBaseUrl);
        if (!webhookResult.IsSuccess)
        {
            return Result<ChannelResponse>.Failure(webhookResult.Error ?? "Failed to register channel webhook.");
        }

        ChannelResponse response = new ChannelResponse(channel.Id, channel.CompanyId, channel.Type, channel.Identifier, channel.IsActive, channel.CreatedAt, channel.UpdatedAt);

        return Result<ChannelResponse>.Success(response);
    }

    public async Task<Result<bool>> DisconnectChannel(Guid companyId, Guid channelId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Channel? channel = await _channelRepository.GetChannelByIdAndCompany(channelId, companyId);
        if (channel == null)
        {
            return Result<bool>.Failure("Channel not found.");
        }

        IChannelProvider? providerDisconnect = null;
        foreach (IChannelProvider provider in _channelProviders)
        {
            if (provider.Type == channel.Type)
            {
                providerDisconnect = provider;
                break;
            }
        }
        if (providerDisconnect != null)
        {
            await providerDisconnect.RemoveWebhook(channel.Identifier);
        }

        await _channelRepository.DeactivateChannel(channelId, companyId);

        return Result<bool>.Success(true);
    }
}