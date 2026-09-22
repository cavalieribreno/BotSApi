using System.Text.Json.Serialization;

namespace BotSaaS.Api.Capabilities.Professionals;

// Lifecycle status of a professional. Persisted as INT in MySQL.
// Serialized as string names ("Active", "Inactive", "OnVacation") over HTTP JSON.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProfessionalStatus
{
    Inactive = 0,
    Active = 1,
    OnVacation = 2
}

// Service provider (barber, stylist, clinician) belonging to a company tenant.
public class Professional
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ProfessionalStatus Status { get; set; } = ProfessionalStatus.Active;
    public DateTime CreatedAt { get; set; }
}

public record CreateProfessionalRequest(string Name);
public record UpdateProfessionalRequest(string Name, ProfessionalStatus Status);
public record ProfessionalResponse(Guid Id, string Name, string Status);
