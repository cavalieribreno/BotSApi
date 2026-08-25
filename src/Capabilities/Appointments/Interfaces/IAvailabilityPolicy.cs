namespace BotSaaS.Api.Capabilities.Appointments;

public interface IAvailabilityPolicy
{
    Task<bool> IsSlotFree(Guid companyId, DateTime scheduledAt);
}