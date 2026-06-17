namespace BotSaaS.Api.Core.Companies;

public interface ICompaniesService
{
    Task<Company> CreateCompany(string name, Segment segment);
}