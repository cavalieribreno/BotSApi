namespace BotSaaS.Api.Core.Companies;

public class CompaniesService : ICompaniesService
{
    private readonly ICompaniesRepository _companieRepository;
    public CompaniesService(ICompaniesRepository companieRepository)
    {
        _companieRepository = companieRepository;
    }

    // Builds a valid Company (owns the creation rules) and persists it.
    public async Task<Company> CreateCompany(string name, Segment segment)
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = name,
            Segment = segment,
            CreatedAt = DateTime.UtcNow
        };

        await _companieRepository.InsertCompany(company);
        return company;
    }
} 