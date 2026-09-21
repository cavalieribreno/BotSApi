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

    public async Task<Company?> GetCompanyById(Guid id)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, name, segment, created_at FROM companies WHERE id = @id";
        command.AddParameter("@id", id.ToString()); //guid

        using DbDataReader reader = await command.ExecuteReaderAsync();

        if(!await reader.ReadAsync()) return null;

        return new Company
        {
            Id = (Guid)reader["id"],
            Name = (string)reader["name"],
            Segment = (Segment)(int)reader["segment"],
            CreatedAt = (DateTime)reader["created_at"]
        };
    }

    public async Task LockCompany(Guid id)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id FROM companies WHERE id = @id FOR UPDATE";
        command.AddParameter("@id", id.ToString());
        await command.ExecuteNonQueryAsync();
    }

    // Fetches operating hours for a specific day of the week (O(1) PK lookup)
    public async Task<BusinessHours?> GetBusinessHours(Guid companyId, DayOfWeek dayOfWeek)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT company_id, day_of_week, opens_at, closes_at, is_closed FROM business_hours WHERE company_id = @company_id AND day_of_week = @day_of_week";
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@day_of_week", (int)dayOfWeek);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new BusinessHours
        {
            CompanyId = (Guid)reader["company_id"],
            DayOfWeek = (DayOfWeek)(int)reader["day_of_week"],
            OpensAt = (TimeSpan)reader["opens_at"],
            ClosesAt = (TimeSpan)reader["closes_at"],
            IsClosed = (bool)reader["is_closed"]
        };
    }

    // Fetches all operating hours of a company (0 to 6)
    public async Task<List<BusinessHours>> GetAllBusinessHours(Guid companyId)
    {
        List<BusinessHours> list = new List<BusinessHours>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT company_id, day_of_week, opens_at, closes_at, is_closed FROM business_hours WHERE company_id = @company_id ORDER BY day_of_week";
        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BusinessHours
            {
                CompanyId = (Guid)reader["company_id"],
                DayOfWeek = (DayOfWeek)(int)reader["day_of_week"],
                OpensAt = (TimeSpan)reader["opens_at"],
                ClosesAt = (TimeSpan)reader["closes_at"],
                IsClosed = (bool)reader["is_closed"]
            });
        }
        return list;
    }

    // Upserts operating hours for a company across the days of the week
    public async Task SaveBusinessHours(Guid companyId, List<BusinessHours> hours)
    {
        foreach (BusinessHours hour in hours)
        {
            using DbCommand command = _dbSession.Connection.CreateCommand();
            command.Transaction = _dbSession.Transaction;
            command.CommandText = @"INSERT INTO business_hours (company_id, day_of_week, opens_at, closes_at, is_closed)
                                    VALUES (@company_id, @day_of_week, @opens_at, @closes_at, @is_closed)
                                    ON DUPLICATE KEY UPDATE
                                        opens_at = VALUES(opens_at),
                                        closes_at = VALUES(closes_at),
                                        is_closed = VALUES(is_closed)";
            command.AddParameter("@company_id", companyId.ToString());
            command.AddParameter("@day_of_week", (int)hour.DayOfWeek);
            command.AddParameter("@opens_at", hour.OpensAt);
            command.AddParameter("@closes_at", hour.ClosesAt);
            command.AddParameter("@is_closed", hour.IsClosed);

            await command.ExecuteNonQueryAsync();
        }
    }
}