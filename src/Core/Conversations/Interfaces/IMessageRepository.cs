namespace BotSaaS.Api.Core.Conversations;

public interface IMessageRepository
{
    public Task InsertMessage(Message message);
    public Task<List<Message>> GetMessagesByConversation(Guid conversationId);
}