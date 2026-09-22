namespace BotSaaS.Api.Capabilities.Appointments;

public interface IAvailabilityPolicy
{
    Task<bool> IsSlotFree(Guid companyId, Guid professionalId, DateTime scheduledAt);
}