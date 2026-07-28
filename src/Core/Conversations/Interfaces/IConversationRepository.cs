namespace BotSaaS.Api.Core.Conversations;

// Contract for conversation data access -- talks SQL.
public interface IConversationRepository
{
    public Task InsertConversation(Conversation conversation);
    public Task<Conversation?> GetConversationByCompanyAndPhone(Guid companyId, string phone);
}