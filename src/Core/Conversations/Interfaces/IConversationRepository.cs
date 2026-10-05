namespace BotSaaS.Api.Core.Conversations;

// Contract for conversation data access -- talks SQL.
public interface IConversationRepository
{
    public Task InsertConversation(Conversation conversation);
    public Task<Conversation?> GetConversationByCompanyAndChannel(Guid companyId, MessageChannel channel, string channelContactId);
    public Task<Conversation?> GetConversationByIdAndCompany(Guid id, Guid companyId);
    public Task CloseConversation(Guid id, Guid companyId);
}