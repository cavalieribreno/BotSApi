using System.Data.Common;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Capabilities.Professionals;

// Talks to MySQL professionals table via DbSession.
public class ProfessionalRepository : IProfessionalRepository
{
    private readonly DbSession _dbSession;

    public ProfessionalRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task InsertProfessional(Professional professional)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "INSERT INTO professionals (id, company_id, name, status, created_at) VALUES (@id, @company_id, @name, @status, @created_at)";
        command.AddParameter("@id", professional.Id.ToString());
        command.AddParameter("@company_id", professional.CompanyId.ToString());
        command.AddParameter("@name", professional.Name);
        command.AddParameter("@status", (int)professional.Status);
        command.AddParameter("@created_at", professional.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Professional>> GetProfessionals(Guid companyId, ProfessionalStatus? status = null)
    {
        List<Professional> professionals = new List<Professional>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;

        string queryProfByCompany = "SELECT id, company_id, name, status, created_at FROM professionals WHERE company_id = @company_id";
        if (status.HasValue)
        {
            queryProfByCompany += " AND status = @status";
            command.AddParameter("@status", (int)status.Value);
        }
        queryProfByCompany += " ORDER BY name ASC";

        command.CommandText = queryProfByCompany;
        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            professionals.Add(new Professional
            {
                Id = (Guid)reader["id"],
                CompanyId = (Guid)reader["company_id"],
                Name = (string)reader["name"],
                Status = (ProfessionalStatus)(int)reader["status"],
                CreatedAt = (DateTime)reader["created_at"]
            });
        }
        return professionals;
    }

    public async Task<Professional?> GetProfessionalById(Guid professionalId, Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, company_id, name, status, created_at FROM professionals WHERE id = @id AND company_id = @company_id LIMIT 1";
        command.AddParameter("@id", professionalId.ToString());
        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Professional
            {
                Id = (Guid)reader["id"],
                CompanyId = (Guid)reader["company_id"],
                Name = (string)reader["name"],
                Status = (ProfessionalStatus)(int)reader["status"],
                CreatedAt = (DateTime)reader["created_at"]
            };
        }
        return null;
    }

    public async Task<int> UpdateProfessional(Guid professionalId, Guid companyId, string name, ProfessionalStatus status)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "UPDATE professionals SET name = @name, status = @status WHERE id = @id AND company_id = @company_id";
        command.AddParameter("@name", name);
        command.AddParameter("@status", (int)status);
        command.AddParameter("@id", professionalId.ToString());
        command.AddParameter("@company_id", companyId.ToString());

        return await command.ExecuteNonQueryAsync();
    }
}
