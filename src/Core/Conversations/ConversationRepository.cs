namespace BotSaaS.Api.Core.Conversations;

using System.Data.Common;
using BotSaaS.Api.Shared.Database;

// Dumb repo: runs SQL on the connection/transaction held by the shared session.
public class ConversationRepository : IConversationRepository
{
    private readonly DbSession _dbSession;
    public ConversationRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }
    public async Task InsertConversation(Conversation conversation)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "INSERT INTO conversations (id, company_id, customer_phone, created_at) VALUES (@id, @company_id, @customer_phone, @created_at)";
        command.AddParameter("@id", conversation.Id.ToString());
        command.AddParameter("@company_id", conversation.CompanyId.ToString()); // FK, guid
        command.AddParameter("@customer_phone", conversation.CustomerPhone);
        command.AddParameter("@created_at", conversation.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }
    // Find the thread for a (company, phone) pair. null = new customer, no thread yet.
    public async Task<Conversation?> GetConversationByCompanyAndPhone(Guid companyId, string phone)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, company_id, customer_phone, created_at FROM conversations WHERE company_id = @company_id AND customer_phone = @customer_phone";
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@customer_phone", phone);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if(!await reader.ReadAsync()) return null;

        return new Conversation
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            CustomerPhone = (string)reader["customer_phone"],
            CreatedAt = (DateTime)reader["created_at"]
        };
    }
}