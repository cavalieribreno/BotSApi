namespace BotSaaS.Api.Core.Conversations;

public interface IConversationRepository
{
    public Task InsertConversation(Conversation conversation);
    public Task<Conversation?> GetConversationByCompanyAndPhone(Guid companyId, string phone);
}