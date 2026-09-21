using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Companies;

// Contract for company domain operations.
public interface ICompaniesService
{
    Task<Company> CreateCompany(string name, Segment segment);
    Task<Company?> GetCompanyById(Guid id);
    Task<List<BusinessHoursResponse>> GetBusinessHours(Guid companyId);
    Task<Result<bool>> UpdateBusinessHours(Guid companyId, List<BusinessHoursRequest> request);
}