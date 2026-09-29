namespace BotSaaS.Api.Capabilities.Professionals;

// Professional DTOs for API requests and responses.
public record CreateProfessionalRequest(string Name);
public record UpdateProfessionalRequest(string Name, ProfessionalStatus Status);
public record ProfessionalResponse(Guid Id, string Name, string Status);
