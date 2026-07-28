namespace BotSaaS.Api.Core.Conversations;

// Contract for message data access -- talks SQL.
public interface IMessageRepository
{
    public Task InsertMessage(Message message);
    public Task<List<Message>> GetMessagesByConversation(Guid conversationId);
}