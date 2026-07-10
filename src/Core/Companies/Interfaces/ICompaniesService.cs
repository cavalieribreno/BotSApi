namespace BotSaaS.Api.Core.Companies;

// Contract for company domain operations.
public interface ICompaniesService
{
    Task<Company> CreateCompany(string name, Segment segment);
}