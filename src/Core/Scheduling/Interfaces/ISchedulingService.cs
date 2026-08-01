using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Scheduling;

// Contract for scheduling domain operations.
public interface ISchedulingService
{
    Task<Result<Appointment>> CreateAppointment(Guid companyId, Guid conversationId, string serviceName, string customerName, string data, string hora);
}
