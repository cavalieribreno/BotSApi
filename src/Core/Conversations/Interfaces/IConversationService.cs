using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Conversations;

// Contract for the conversation flow (one bot turn).
public interface IConversationService
{
    public Task<Result<string>> ProcessMessage(Guid companyId, string customerPhone, string messageText);
    public Task<Result<ConversationMessages>> GetMessages(Guid companyId, Guid conversationId);
}