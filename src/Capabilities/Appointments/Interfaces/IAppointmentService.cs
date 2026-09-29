using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Capabilities.Appointments;

// Contract for scheduling domain operations.
public interface IAppointmentService
{
    Task<Result<Appointment>> CreateAppointment(Guid companyId, CreateAppointmentRequest request, AppointmentOrigin origin, Guid? conversationId = null);
    Task<List<Appointment>> GetAppointments(Guid companyId, DateOnly date);
    Task<List<Appointment>> GetAppointmentsByCustomer(Guid companyId, string customerPhone);
    Task<Result<bool>> UpdateAppointmentStatus(Guid appointmentId, Guid companyId, AppointmentStatus status);
}
