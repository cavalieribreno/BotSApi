using System.Data.Common;
using System.Text.RegularExpressions;
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

    private static string CleanPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        string clean = Regex.Replace(phone, @"\D", "");

        if (clean.StartsWith("55") && (clean.Length == 12 || clean.Length == 13))
        {
            clean = clean.Substring(2);
        }

        return clean;
    }

    // Resolves an existing customer by phone or creates a new record.
    public async Task<Result<Customer>> FindOrCreateCustomer(Guid companyId, string phone, string name)
    {
        string cleanPhone = CleanPhone(phone);

        if (cleanPhone.Length < 10 || cleanPhone.Length > 11)
        {
            return Result<Customer>.Failure("Telefone inválido. Informe um número com DDD (10 ou 11 dígitos).");
        }

        string cleanName = string.Empty;
        if (name != null)
        {
            cleanName = name.Trim();
        }

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            return Result<Customer>.Failure("Nome do cliente é obrigatório.");
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        _dbSession.Transaction = null!;
        await connection.OpenAsync();

        Customer? customer = await _customerRepository.GetCustomerByPhone(companyId, cleanPhone);

        if (customer != null)
        {
            return Result<Customer>.Success(customer);
        }

        Customer newCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = cleanName,
            Phone = cleanPhone,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.InsertCustomer(newCustomer);

        return Result<Customer>.Success(newCustomer);
    }

    public async Task<Customer?> GetCustomerByPhone(Guid companyId, string phone)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        string cleanPhone = CleanPhone(phone);

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

    public async Task<Result<CustomerResponse>> UpdateCustomer(Guid id, Guid companyId, UpdateCustomerRequest request)
    {
        if (request == null)
        {
            return Result<CustomerResponse>.Failure("Dados do cliente são obrigatórios.");
        }

        string cleanName = string.Empty;
        if (request.Name != null)
        {
            cleanName = request.Name.Trim();
        }

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            return Result<CustomerResponse>.Failure("Nome do cliente é obrigatório.");
        }

        string cleanPhone = CleanPhone(request.Phone);

        if (cleanPhone.Length < 10 || cleanPhone.Length > 11)
        {
            return Result<CustomerResponse>.Failure("Telefone inválido. Informe um número com DDD (10 ou 11 dígitos).");
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Customer? existingCustomer = await _customerRepository.GetCustomerById(id, companyId);
        if (existingCustomer == null)
        {
            return Result<CustomerResponse>.Failure("Cliente não encontrado.");
        }

        if (existingCustomer.Phone != cleanPhone)
        {
            Customer? customerWithSamePhone = await _customerRepository.GetCustomerByPhone(companyId, cleanPhone);
            if (customerWithSamePhone != null && customerWithSamePhone.Id != id)
            {
                return Result<CustomerResponse>.Failure("Já existe outro cliente cadastrado com este telefone.");
            }
        }

        int rowsAffected = await _customerRepository.UpdateCustomer(id, companyId, cleanName, cleanPhone);
        if (rowsAffected == 0)
        {
            return Result<CustomerResponse>.Failure("Erro ao atualizar cliente.");
        }

        CustomerResponse response = new CustomerResponse(id, cleanName, cleanPhone, existingCustomer.CreatedAt);
        return Result<CustomerResponse>.Success(response);
    }
}