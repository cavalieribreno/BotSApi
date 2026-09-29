using System.Data.Common;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Core.Customers;

// Tenant-isolated customer persistence and query repository.
public class CustomerRepository : ICustomerRepository
{
    private readonly DbSession _dbSession;

    public CustomerRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task InsertCustomer(Customer customer)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"INSERT INTO customers (id, company_id, name, phone, created_at) 
                                VALUES (@id, @company_id, @name, @phone, @created_at)";

        command.AddParameter("@id", customer.Id.ToString());
        command.AddParameter("@company_id", customer.CompanyId.ToString());
        command.AddParameter("@name", customer.Name);
        command.AddParameter("@phone", customer.Phone);
        command.AddParameter("@created_at", customer.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<Customer?> GetCustomerByPhone(Guid companyId, string phone)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT id, company_id, name, phone, created_at 
                                FROM customers 
                                WHERE company_id = @company_id AND phone = @phone";

        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@phone", phone);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Customer
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            Name = (string)reader["name"],
            Phone = (string)reader["phone"],
            CreatedAt = (DateTime)reader["created_at"]
        };
    }

    public async Task<List<Customer>> GetCustomersByCompany(Guid companyId)
    {
        List<Customer> customers = new List<Customer>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT id, company_id, name, phone, created_at 
                                FROM customers 
                                WHERE company_id = @company_id 
                                ORDER BY name ASC";

        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(new Customer
            {
                Id = (Guid)reader["id"],
                CompanyId = (Guid)reader["company_id"],
                Name = (string)reader["name"],
                Phone = (string)reader["phone"],
                CreatedAt = (DateTime)reader["created_at"]
            });
        }

        return customers;
    }

    public async Task<Customer?> GetCustomerById(Guid id, Guid companyId)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT id, company_id, name, phone, created_at 
                                FROM customers 
                                WHERE id = @id AND company_id = @company_id";

        command.AddParameter("@id", id.ToString());
        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Customer
        {
            Id = (Guid)reader["id"],
            CompanyId = (Guid)reader["company_id"],
            Name = (string)reader["name"],
            Phone = (string)reader["phone"],
            CreatedAt = (DateTime)reader["created_at"]
        };
    }

    public async Task<int> UpdateCustomerName(Guid id, Guid companyId, string name)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"UPDATE customers 
                                SET name = @name 
                                WHERE id = @id AND company_id = @company_id";

        command.AddParameter("@id", id.ToString());
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@name", name);

        return await command.ExecuteNonQueryAsync();
    }
}
