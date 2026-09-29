using System.Data.Common;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Customers;

// Customer domain operations: manages customer identity and tenant isolation.
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IDatabase _databaseConnection;
    private readonly DbSession _dbSession;

    public CustomerService(ICustomerRepository customerRepository, IDatabase databaseConnection, DbSession dbSession)
    {
        _customerRepository = customerRepository;
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
    }

    // Resolves an existing customer by phone or creates a new record.
    public async Task<Result<Customer>> FindOrCreateCustomer(Guid companyId, string phone, string name)
    {
        string cleanPhone = string.Empty;
        if (phone != null)
        {
            cleanPhone = phone.Trim();
        }

        if (string.IsNullOrWhiteSpace(cleanPhone))
        {
            return Result<Customer>.Failure("Phone is required.");
        }

        string cleanName = string.Empty;
        if (name != null)
        {
            cleanName = name.Trim();
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Customer? customer = await _customerRepository.GetCustomerByPhone(companyId, cleanPhone);

        if (customer != null)
        {
            return Result<Customer>.Success(customer);
        }

        customer = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = cleanName,
            Phone = cleanPhone,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.InsertCustomer(customer);
        return Result<Customer>.Success(customer);
    }

    public async Task<Customer?> GetCustomerByPhone(Guid companyId, string phone)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        string cleanPhone = string.Empty;
        if (phone != null)
        {
            cleanPhone = phone.Trim();
        }

        return await _customerRepository.GetCustomerByPhone(companyId, cleanPhone);
    }

    public async Task<List<CustomerResponse>> GetCustomers(Guid companyId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        List<Customer> customers = await _customerRepository.GetCustomersByCompany(companyId);
        List<CustomerResponse> response = new List<CustomerResponse>();

        foreach (Customer customer in customers)
        {
            response.Add(new CustomerResponse(customer.Id, customer.Name, customer.Phone, customer.CreatedAt));
        }

        return response;
    }

    public async Task<Result<Customer>> GetCustomerById(Guid id, Guid companyId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Customer? customer = await _customerRepository.GetCustomerById(id, companyId);
        if (customer == null)
        {
            return Result<Customer>.Failure("Customer not found.");
        }

        return Result<Customer>.Success(customer);
    }

    public async Task<Result<bool>> UpdateCustomerName(Guid id, Guid companyId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<bool>.Failure("Name is required.");
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        int rowsAffected = await _customerRepository.UpdateCustomerName(id, companyId, name.Trim());
        if (rowsAffected == 0)
        {
            return Result<bool>.Failure("Customer not found.");
        }

        return Result<bool>.Success(true);
    }
}