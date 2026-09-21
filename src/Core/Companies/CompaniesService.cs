using System.Data.Common;
using System.Globalization;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Companies;

public class CompaniesService : ICompaniesService
{
    private readonly ICompaniesRepository _companieRepository;
    private readonly IDatabase _databaseConnection;
    private readonly DbSession _dbSession;

    public CompaniesService(ICompaniesRepository companieRepository, IDatabase databaseConnection, DbSession dbSession)
    {
        _companieRepository = companieRepository;
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
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

    // Company lookup by id (used at login to show the tenant's name). null = not found.
    public async Task<Company?> GetCompanyById(Guid id)
    {
        return await _companieRepository.GetCompanyById(id);
    }

    // Retrieves all operating hours of a company formatted for API consumption
    public async Task<List<BusinessHoursResponse>> GetBusinessHours(Guid companyId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        List<BusinessHours> hours = await _companieRepository.GetAllBusinessHours(companyId);

        List<BusinessHoursResponse> response = new List<BusinessHoursResponse>();
        foreach (BusinessHours hour in hours)
        {
            response.Add(new BusinessHoursResponse(
                (int)hour.DayOfWeek,
                hour.OpensAt.ToString(@"hh\:mm"),
                hour.ClosesAt.ToString(@"hh\:mm"),
                hour.IsClosed
            ));
        }
        return response;
    }

    // Validates and saves operating hours for a company
    public async Task<Result<bool>> UpdateBusinessHours(Guid companyId, List<BusinessHoursRequest> request)
    {
        if (request is null || request.Count == 0)
        {
            return Result<bool>.Failure("Horários de funcionamento inválidos.");
        }

        List<BusinessHours> domainHours = new List<BusinessHours>();

        foreach (BusinessHoursRequest item in request)
        {
            if (item.DayOfWeek < 0 || item.DayOfWeek > 6)
            {
                return Result<bool>.Failure("Dia da semana inválido.");
            }

            TimeSpan opensAt = TimeSpan.Zero;
            TimeSpan closesAt = TimeSpan.Zero;

            if (!item.IsClosed)
            {
                if (!TimeSpan.TryParseExact(item.OpensAt, @"hh\:mm", CultureInfo.InvariantCulture, out opensAt) ||
                    !TimeSpan.TryParseExact(item.ClosesAt, @"hh\:mm", CultureInfo.InvariantCulture, out closesAt))
                {
                    return Result<bool>.Failure("Formato de horário inválido. Use HH:mm.");
                }

                if (opensAt >= closesAt)
                {
                    return Result<bool>.Failure("O horário de abertura deve ser anterior ao de fechamento.");
                }
            }

            domainHours.Add(new BusinessHours
            {
                CompanyId = companyId,
                DayOfWeek = (DayOfWeek)item.DayOfWeek,
                OpensAt = opensAt,
                ClosesAt = closesAt,
                IsClosed = item.IsClosed
            });
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        using DbTransaction transaction = await connection.BeginTransactionAsync();
        _dbSession.Transaction = transaction;

        await _companieRepository.SaveBusinessHours(companyId, domainHours);
        await transaction.CommitAsync();

        return Result<bool>.Success(true);
    }
}