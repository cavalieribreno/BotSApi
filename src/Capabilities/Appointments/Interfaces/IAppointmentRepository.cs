namespace BotSaaS.Api.Capabilities.Appointments;

// Contract for appointment data access - talks SQL.
public interface IAppointmentRepository
{
    public Task InsertAppointment(Appointment appointment);
    public Task<List<Appointment>> GetAppointmentsByCompany(Guid companyId, DateOnly date);
    public Task<List<Appointment>> GetAppointmentsByCustomer(Guid companyId, string customerPhone);
    public Task<int> UpdateAppointmentStatus(Guid appointmentId, Guid companyId, AppointmentStatus status);
    public Task<bool> SlotTaken(Guid companyId, Guid professionalId, DateTime start, DateTime end);
    public Task<List<UpcomingReminder>> GetPendingReminders(DateTime windowStart, DateTime windowEnd);
    public Task MarkReminded(Guid appointmentId, DateTime remindedAt);
}