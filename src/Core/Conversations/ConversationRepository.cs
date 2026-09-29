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
        command.CommandText = "INSERT INTO conversations (id, company_id, customer_phone, is_active, created_at) VALUES (@id, @company_id, @customer_phone, @is_active, @created_at)";
        command.AddParameter("@id", conversation.Id.ToString());
        command.AddParameter("@company_id", conversation.CompanyId.ToString()); // FK, guid
        command.AddParameter("@customer_phone", conversation.CustomerPhone);
        command.AddParameter("@is_active", conversation.IsActive);
        command.AddParameter("@created_at", conversation.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }
    // Find the thread for a (company, phone) pair. null = new customer, no thread yet.
    public async Task<Conversation?> GetConversationByCompanyAndPhone(Guid companyId, string phone)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, company_id, customer_phone, is_active, created_at, closed_at FROM conversations WHERE company_id = @company_id AND customer_phone = @customer_phone AND is_active = 1 LIMIT 1";
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@customer_phone", phone);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        DateTime? closedAt = null;
        if (reader["closed_at"] != DBNull.Value)
        {
            closedAt = (DateTime)reader["closed_at"];
        }

        return new Conversation
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            CustomerPhone = (string)reader["customer_phone"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            CreatedAt = (DateTime)reader["created_at"],
            ClosedAt = closedAt
        };
    }

    // Get a conversation by id, scoped to a company. null = not found / not this company (tenant guard).
    public async Task<Conversation?> GetConversationByIdAndCompany(Guid id, Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, company_id, customer_phone, is_active, created_at, closed_at FROM conversations WHERE id = @id AND company_id = @company_id";
        command.AddParameter("@id", id.ToString());
        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        DateTime? closedAt = null;
        if (reader["closed_at"] != DBNull.Value)
        {
            closedAt = (DateTime)reader["closed_at"];
        }

        return new Conversation
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            CustomerPhone = (string)reader["customer_phone"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            CreatedAt = (DateTime)reader["created_at"],
            ClosedAt = closedAt
        };
    }

    // Closes an active conversation thread by setting is_active = 0 and closed_at = current time.
    public async Task CloseConversation(Guid id, Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "UPDATE conversations SET is_active = 0, closed_at = @closed_at WHERE id = @id AND company_id = @company_id AND is_active = 1";
        command.AddParameter("@id", id.ToString());
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@closed_at", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync();
    }
}