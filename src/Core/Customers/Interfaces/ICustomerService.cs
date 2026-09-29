using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Customers;

public interface ICustomerService
{
    Task<Result<Customer>> FindOrCreateCustomer(Guid companyId, string phone, string name);
    Task<Customer?> GetCustomerByPhone(Guid companyId, string phone);
    Task<List<CustomerResponse>> GetCustomers(Guid companyId);
    Task<Result<Customer>> GetCustomerById(Guid id, Guid companyId);
    Task<Result<bool>> UpdateCustomerName(Guid id, Guid companyId, string name);
}
