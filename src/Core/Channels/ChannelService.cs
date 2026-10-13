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

        string? appBaseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL");
        if (string.IsNullOrWhiteSpace(appBaseUrl))
        {
            return Result<ChannelResponse>.Failure("Environment variable APP_BASE_URL is not configured.");
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync(); 

        // Check if channel already exists in database
        Channel? existingChannel = await _channelRepository.GetChannelByIdentifier(type, identifier);
        if (existingChannel != null)
        {
            if (existingChannel.IsActive)
            {
                return Result<ChannelResponse>.Failure("Este canal já está conectado e ativo.");
            }

            if (existingChannel.CompanyId != companyId)
            {
                return Result<ChannelResponse>.Failure("Este canal já está vinculado a outra organização.");
            }

            // Reactivate existing channel
            await _channelRepository.ReactivateChannel(existingChannel.Id, companyId);

            Result<bool> webhookResult = await providerConnect.RegisterWebhook(existingChannel.Id, identifier, appBaseUrl);
            if (!webhookResult.IsSuccess)
            {
                return Result<ChannelResponse>.Failure(webhookResult.Error ?? "Failed to register channel webhook.");
            }

            ChannelResponse reactivatedResponse = new ChannelResponse(existingChannel.Id, existingChannel.CompanyId, existingChannel.Type, existingChannel.Identifier, true, existingChannel.CreatedAt, DateTime.UtcNow);

            return Result<ChannelResponse>.Success(reactivatedResponse);
        }

        // New channel registration
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

        await _channelRepository.InsertChannel(channel);

        Result<bool> newWebhookResult = await providerConnect.RegisterWebhook(channel.Id, identifier, appBaseUrl);
        if (!newWebhookResult.IsSuccess)
        {
            return Result<ChannelResponse>.Failure(newWebhookResult.Error ?? "Failed to register channel webhook.");
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