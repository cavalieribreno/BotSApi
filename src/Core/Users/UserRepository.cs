using System.Data.Common;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Core.Users;

// Dumb repo: runs SQL on the connection/transaction held by the shared session.
public class UserRepository : IUserRepository
{
    private readonly DbSession _dbSession;
    public UserRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task InsertUser(User user)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "INSERT INTO users (id, company_id, name, email, password_hash, created_at) VALUES (@id, @company_id, @name, @email, @password_hash, @created_at)";
        command.AddParameter("@id", user.Id.ToString());
        command.AddParameter("@company_id", user.CompanyId.ToString()); // FK, guid
        command.AddParameter("@name", user.Name);
        command.AddParameter("@email", user.Email);
        command.AddParameter("@password_hash", user.PasswordHash);
        command.AddParameter("@created_at", user.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, company_id, name, email, password_hash, created_at FROM users WHERE email = @email";
        command.AddParameter("@email", email);

        using DbDataReader reader = await command.ExecuteReaderAsync();

        if(!await reader.ReadAsync()) return null;

        return new User
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            Name = (string)reader["name"],
            Email = (string)reader["email"],
            PasswordHash = (string)reader["password_hash"],
            CreatedAt = (DateTime)reader["created_at"]
        };
    }
}