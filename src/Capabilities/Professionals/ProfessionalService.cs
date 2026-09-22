using System.Data.Common;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Capabilities.Professionals;

// Professional domain operations.
public class ProfessionalService : IProfessionalService
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IDatabase _databaseConnection;
    private readonly DbSession _dbSession;

    public ProfessionalService(IProfessionalRepository professionalRepository, IDatabase databaseConnection, DbSession dbSession)
    {
        _professionalRepository = professionalRepository;
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
    }

    public async Task<Result<Professional>> CreateProfessional(Guid companyId, CreateProfessionalRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<Professional>.Failure("Name is required.");
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Professional professional = new Professional
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = request.Name.Trim(),
            Status = ProfessionalStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _professionalRepository.InsertProfessional(professional);
        return Result<Professional>.Success(professional);
    }

    public async Task<List<Professional>> GetProfessionals(Guid companyId, ProfessionalStatus? status = null)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        return await _professionalRepository.GetProfessionals(companyId, status);
    }

    public async Task<Result<Professional>> GetProfessionalById(Guid professionalId, Guid companyId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Professional? professional = await _professionalRepository.GetProfessionalById(professionalId, companyId);
        if (professional is null)
        {
            return Result<Professional>.Failure("Professional not found.");
        }

        return Result<Professional>.Success(professional);
    }

    public async Task<Result<bool>> UpdateProfessional(Guid professionalId, Guid companyId, UpdateProfessionalRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<bool>.Failure("Name is required.");   
        }

        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        int rows = await _professionalRepository.UpdateProfessional(professionalId, companyId, request.Name.Trim(), request.Status);
        if (rows == 0)
        {
            return Result<bool>.Failure("Professional not found.");
        }
        return Result<bool>.Success(true);
    }
}
