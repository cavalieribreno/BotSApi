namespace BotSaaS.Api.Core.Customers;

// Data access contract for customer records (tenant-isolated).
public interface ICustomerRepository
{
    Task InsertCustomer(Customer customer);
    Task<Customer?> GetCustomerByPhone(Guid companyId, string phone);
    Task<List<Customer>> GetCustomersByCompany(Guid companyId);
    Task<Customer?> GetCustomerById(Guid id, Guid companyId);
    Task<int> UpdateCustomerName(Guid id, Guid companyId, string name);
}
