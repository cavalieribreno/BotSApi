using System.Data.Common;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Core.Channels;

public class ChannelRepository : IChannelRepository
{
    private readonly DbSession _dbSession;

    public ChannelRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task InsertChannel(Channel channel)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"INSERT INTO channels (id, company_id, type, identifier, is_active, created_at, updated_at) 
                                VALUES (@id, @company_id, @type, @identifier, @is_active, @created_at, @updated_at)";

        command.AddParameter("@id", channel.Id.ToString());
        command.AddParameter("@company_id", channel.CompanyId.ToString());
        command.AddParameter("@type", (int)channel.Type);
        command.AddParameter("@identifier", channel.Identifier);
        command.AddParameter("@is_active", channel.IsActive);
        command.AddParameter("@created_at", channel.CreatedAt);
        command.AddParameter("@updated_at", channel.UpdatedAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Channel>> GetChannelsByCompany(Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT id, company_id, type, identifier, is_active, created_at, updated_at 
                                FROM channels 
                                WHERE company_id = @company_id AND is_active = 1";

        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        List<Channel> channels = new List<Channel>();

        while (await reader.ReadAsync())
        {
            channels.Add(new Channel
            {
                Id = (Guid)reader["id"],
                CompanyId = (Guid)reader["company_id"],
                Type = (MessageChannel)Convert.ToInt32(reader["type"]),
                Identifier = (string)reader["identifier"],
                IsActive = Convert.ToBoolean(reader["is_active"]),
                CreatedAt = (DateTime)reader["created_at"],
                UpdatedAt = (DateTime)reader["updated_at"]
            });
        }

        return channels;
    }

    // Scoped tenant lookup: enforces isolation to prevent cross-tenant access (IDOR defense).
    public async Task<Channel?> GetChannelByIdAndCompany(Guid id, Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT id, company_id, type, identifier, is_active, created_at, updated_at 
                                FROM channels 
                                WHERE id = @id AND company_id = @company_id";

        command.AddParameter("@id", id.ToString());
        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Channel
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            Type = (MessageChannel)Convert.ToInt32(reader["type"]),
            Identifier = (string)reader["identifier"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            CreatedAt = (DateTime)reader["created_at"],
            UpdatedAt = (DateTime)reader["updated_at"]
        };
    }

    // Public webhook tenant resolution: resolves active channel by id without requiring companyId context upfront.
    public async Task<Channel?> GetActiveChannelById(Guid id)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT id, company_id, type, identifier, is_active, created_at, updated_at 
                                FROM channels 
                                WHERE id = @id AND is_active = 1 
                                LIMIT 1";

        command.AddParameter("@id", id.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Channel
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            Type = (MessageChannel)Convert.ToInt32(reader["type"]),
            Identifier = (string)reader["identifier"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            CreatedAt = (DateTime)reader["created_at"],
            UpdatedAt = (DateTime)reader["updated_at"]
        };
    }

    public async Task<Channel?> GetChannelByIdentifier(MessageChannel type, string identifier)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT id, company_id, type, identifier, is_active, created_at, updated_at 
                                FROM channels 
                                WHERE type = @type AND identifier = @identifier 
                                LIMIT 1";

        command.AddParameter("@type", (int)type);
        command.AddParameter("@identifier", identifier);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Channel
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            Type = (MessageChannel)Convert.ToInt32(reader["type"]),
            Identifier = (string)reader["identifier"],
            IsActive = Convert.ToBoolean(reader["is_active"]),
            CreatedAt = (DateTime)reader["created_at"],
            UpdatedAt = (DateTime)reader["updated_at"]
        };
    }

    public async Task<int> ReactivateChannel(Guid id, Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"UPDATE channels 
                                SET is_active = 1, updated_at = @updated_at 
                                WHERE id = @id AND company_id = @company_id";

        command.AddParameter("@id", id.ToString());
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@updated_at", DateTime.UtcNow);

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> DeactivateChannel(Guid id, Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"UPDATE channels 
                                SET is_active = 0, updated_at = @updated_at 
                                WHERE id = @id AND company_id = @company_id";

        command.AddParameter("@id", id.ToString());
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@updated_at", DateTime.UtcNow);

        return await command.ExecuteNonQueryAsync();
    }
}