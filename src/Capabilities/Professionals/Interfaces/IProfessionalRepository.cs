namespace BotSaaS.Api.Capabilities.Professionals;

// Contract for professional data access - talks SQL.
public interface IProfessionalRepository
{
    Task InsertProfessional(Professional professional);
    Task<List<Professional>> GetProfessionals(Guid companyId, ProfessionalStatus? status = null);
    Task<Professional?> GetProfessionalById(Guid professionalId, Guid companyId);
    Task<int> UpdateProfessional(Guid professionalId, Guid companyId, string name, ProfessionalStatus status);
}
