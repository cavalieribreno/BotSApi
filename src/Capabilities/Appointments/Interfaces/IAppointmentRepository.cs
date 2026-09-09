namespace BotSaaS.Api.Capabilities.Appointments;

// Contract for appointment data access - talks SQL.
public interface IAppointmentRepository
{
    public Task InsertAppointment(Appointment appointment);
    public Task<List<Appointment>> GetAppointmentsByCompany(Guid companyId);
    public Task<List<Appointment>> GetAppointmentsByCustomer(Guid companyId, string customerPhone);
    public Task<int> UpdateAppointmentStatus(Guid appointmentId, Guid companyId, AppointmentStatus status);
    public Task<bool> SlotTaken(Guid companyId, DateTime start, DateTime end);
}