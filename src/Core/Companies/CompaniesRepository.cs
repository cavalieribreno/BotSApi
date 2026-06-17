using System.Data.Common;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Core.Companies;

// Dumb repo: runs SQL on the connection/transaction held by the shared session.
public class CompaniesRepository : ICompaniesRepository
{
    private readonly DbSession _dbSession;
    public CompaniesRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task InsertCompany(Company company)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "INSERT INTO companies (id, name, segment, created_at) VALUES (@id, @name, @segment, @created_at)";
        command.AddParameter("@id", company.Id.ToString()); //guid
        command.AddParameter("@name", company.Name);
        command.AddParameter("@segment", (int)company.Segment);
        command.AddParameter("@created_at", company.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }
}