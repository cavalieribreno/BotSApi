using System.Data.Common;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Channels.Telegram;

[AllowAnonymous]
[ApiController]
[Route("api/webhooks/telegram")]
public class TelegramWebhookController : ControllerBase
{
    private readonly IChannelRepository _channelRepository;
    private readonly IConversationService _conversationService;
    private readonly IMessageSender _messageSender;
    private readonly IDatabase _databaseConnection;
    private readonly DbSession _dbSession;

    public TelegramWebhookController(IChannelRepository channelRepository, IConversationService conversationService, IMessageSender messageSender, IDatabase databaseConnection, DbSession dbSession)
    {
        _channelRepository = channelRepository;
        _conversationService = conversationService;
        _messageSender = messageSender;
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
    }

    [HttpPost("{channelId:guid}")]
    public async Task<IActionResult> HandleWebhook(Guid channelId, [FromBody] TgUpdate update)
    {
        // 1. Ignore updates that do not contain a text message
        if (update.Message?.Text is not string text || string.IsNullOrWhiteSpace(text))
        {
            return Ok();
        }

        // 2. Open DB connection and resolve active tenant channel
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Channel? channel = await _channelRepository.GetActiveChannelById(channelId);
        if (channel == null)
        {
            return Ok();
        }

        long chatId = update.Message.Chat.Id;

        // 3. Process the incoming message via conversation AI flow
        Result<string> result = await _conversationService.ProcessMessage(channel.CompanyId, MessageChannel.Telegram, chatId.ToString(), text);

        string replyText = "Desculpe, tive um problema aqui. Pode tentar de novo?";
        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Value))
        {
            replyText = result.Value;
        }

        // 4. Send response back to the customer on Telegram
        await _messageSender.SendMessageAsync(chatId.ToString(), replyText, channel.Identifier);

        return Ok();
    }
}
