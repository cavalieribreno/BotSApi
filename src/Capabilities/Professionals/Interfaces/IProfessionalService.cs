using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Capabilities.Professionals;

// Contract for professional domain operations.
public interface IProfessionalService
{
    Task<Result<Professional>> CreateProfessional(Guid companyId, CreateProfessionalRequest request);
    Task<List<Professional>> GetProfessionals(Guid companyId, ProfessionalStatus? status = null);
    Task<Result<Professional>> GetProfessionalById(Guid professionalId, Guid companyId);
    Task<Result<bool>> UpdateProfessional(Guid professionalId, Guid companyId, UpdateProfessionalRequest request);
}
