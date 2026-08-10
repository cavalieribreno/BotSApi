using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Capabilities.Appointments;

// Contract for scheduling domain operations.
public interface IAppointmentService
{
    Task<Result<Appointment>> CreateAppointment(Guid companyId, Guid conversationId, string serviceName, string customerName, string data, string hora);
    Task<List<Appointment>> GetAppointments(Guid companyId);
    Task<Result<bool>> UpdateAppointmentStatus(Guid appointmentId, Guid companyId, AppointmentStatus status);
}
