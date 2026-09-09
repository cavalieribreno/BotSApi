using System.Data.Common;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Core.Conversations;

// Dumb repo: runs SQL on the connection/transaction held by the shared session.
public class MessageRepository : IMessageRepository
{
    private readonly DbSession _dbSession;
    public MessageRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task InsertMessage(Message message)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "INSERT INTO messages (id, conversation_id, role, content, created_at) VALUES (@id, @conversation_id, @role, @content, @created_at)";
        command.AddParameter("@id", message.Id.ToString());
        command.AddParameter("@conversation_id", message.ConversationId.ToString());
        command.AddParameter("@role", (int)message.Role);
        command.AddParameter("@content", message.Content);
        command.AddParameter("@created_at", message.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    // Full history, oldest-first (what the LLM needs to reconstruct context). No cap yet.
    public async Task<List<Message>> GetMessagesByConversation(Guid conversationId)
    {
        List<Message> messages = new List<Message>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, conversation_id, role, content, created_at FROM messages WHERE conversation_id = @conversationId ORDER BY created_at";
        command.AddParameter("@conversationId", conversationId);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while(await reader.ReadAsync())
        {
            Message message = new Message
            {
                Id = (Guid)reader["id"],
                ConversationId = (Guid)reader["conversation_id"],
                Role = (MessageRole)(int)reader["role"],
                Content = (string)reader["content"],
                CreatedAt = (DateTime)reader["created_at"]
            };
            messages.Add(message);
        }
        return messages;
    }
}