namespace BotSaaS.Api.Core.Companies;

// Contract for company data access -- talks SQL
public interface ICompaniesRepository
{
    Task InsertCompany(Company company);
    Task<Company?> GetCompanyById(Guid id);
    Task LockCompany (Guid id);
}