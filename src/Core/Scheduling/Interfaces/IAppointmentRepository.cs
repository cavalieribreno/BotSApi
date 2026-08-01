namespace BotSaaS.Api.Core.Scheduling;

// Contract for appointment data access -- talks SQL.
public interface IAppointmentRepository
{
    public Task InsertAppointment(Appointment appointment);
    public Task<List<Appointment>> GetAppointmentsByCompany(Guid companyId);
    public Task<int> UpdateStatus(Guid appointmentId, Guid companyId, AppointmentStatus status);
}
