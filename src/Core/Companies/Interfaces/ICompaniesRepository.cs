namespace BotSaaS.Api.Core.Companies;

// Contract for company data access -- talks SQL
public interface ICompaniesRepository
{
    Task InsertCompany(Company company);
    Task<Company?> GetCompanyById(Guid id);
    Task LockCompany (Guid id);
    Task<BusinessHours?> GetBusinessHours(Guid companyId, DayOfWeek dayOfWeek);
    Task<List<BusinessHours>> GetAllBusinessHours(Guid companyId);
    Task SaveBusinessHours(Guid companyId, List<BusinessHours> hours);
}