using System.Data.Common;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Capabilities.Appointments;

// Dumb repo: runs SQL on the connection/transaction held by the shared session.
public class AppointmentRepository : IAppointmentRepository
{
    private readonly DbSession _dbSession;
    public AppointmentRepository(DbSession dbSession)
    {
        _dbSession = dbSession;
    }

    public async Task InsertAppointment(Appointment appointment)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "INSERT INTO appointments (id, company_id, conversation_id, professional_id, service_name, customer_name, scheduled_at, status, created_at) VALUES (@id, @company_id, @conversation_id, @professional_id, @service_name, @customer_name, @scheduled_at, @status, @created_at)";
        command.AddParameter("@id", appointment.Id.ToString());
        command.AddParameter("@company_id", appointment.CompanyId.ToString());       // FK, guid
        command.AddParameter("@conversation_id", appointment.ConversationId.ToString()); // FK, guid
        command.AddParameter("@professional_id", appointment.ProfessionalId.ToString()); // FK, guid
        command.AddParameter("@service_name", appointment.ServiceName);
        command.AddParameter("@customer_name", appointment.CustomerName);
        command.AddParameter("@scheduled_at", appointment.ScheduledAt);
        command.AddParameter("@status", (int)appointment.Status);
        command.AddParameter("@created_at", appointment.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    // Appointments of a company on a specific date, soonest-first.
    // Uses [start, end) interval for index efficiency (SARGable).
    public async Task<List<Appointment>> GetAppointmentsByCompany(Guid companyId, DateOnly date)
    {
        List<Appointment> appointments = new List<Appointment>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;

        DateTime start = date.ToDateTime(TimeOnly.MinValue);
        DateTime end = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

        command.CommandText = @"SELECT ap.id, ap.company_id, ap.conversation_id, ap.professional_id, pr.name AS professional_name, 
                                       ap.service_name, ap.customer_name, ap.scheduled_at, ap.status, ap.reminded_at, ap.created_at 
                                FROM appointments ap
                                INNER JOIN professionals pr ON ap.professional_id = pr.id
                                WHERE ap.company_id = @company_id
                                  AND ap.scheduled_at >= @start 
                                  AND ap.scheduled_at < @end
                                ORDER BY ap.scheduled_at";

        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@start", start);
        command.AddParameter("@end", end);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Appointment appointment = new Appointment
            {
                Id = (Guid)reader["id"],
                CompanyId = (Guid)reader["company_id"],
                ConversationId = (Guid)reader["conversation_id"],
                ProfessionalId = (Guid)reader["professional_id"],
                ProfessionalName = (string)reader["professional_name"],
                ServiceName = (string)reader["service_name"],
                CustomerName = (string)reader["customer_name"],
                ScheduledAt = (DateTime)reader["scheduled_at"],
                Status = (AppointmentStatus)(int)reader["status"],
                RemindedAt = reader["reminded_at"] == DBNull.Value ? null : (DateTime?)reader["reminded_at"],
                CreatedAt = (DateTime)reader["created_at"]
            };
            appointments.Add(appointment);
        }
        return appointments;
    }

    // All appointments of one customer (company + phone), soonest-first. Joins conversations because the
    // phone lives there, not on the appointment - survives conversation rotation (same phone, new conversation).
    public async Task<List<Appointment>> GetAppointmentsByCustomer(Guid companyId, string customerPhone)
    {
        List<Appointment> appointments = new List<Appointment>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT ap.id, ap.company_id, ap.conversation_id, ap.professional_id, pr.name AS professional_name, 
                                       ap.service_name, ap.customer_name, ap.scheduled_at, ap.status, ap.reminded_at, ap.created_at
                                FROM appointments ap
                                JOIN conversations cv ON ap.conversation_id = cv.id
                                INNER JOIN professionals pr ON ap.professional_id = pr.id
                                WHERE ap.company_id = @company_id AND cv.customer_phone = @customer_phone
                                ORDER BY ap.scheduled_at";
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@customer_phone", customerPhone);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Appointment appointment = new Appointment
            {
                Id = (Guid)reader["id"],
                CompanyId = (Guid)reader["company_id"],
                ConversationId = (Guid)reader["conversation_id"],
                ProfessionalId = (Guid)reader["professional_id"],
                ProfessionalName = (string)reader["professional_name"],
                ServiceName = (string)reader["service_name"],
                CustomerName = (string)reader["customer_name"],
                ScheduledAt = (DateTime)reader["scheduled_at"],
                Status = (AppointmentStatus)(int)reader["status"],
                RemindedAt = reader["reminded_at"] == DBNull.Value ? null : (DateTime?)reader["reminded_at"],
                CreatedAt = (DateTime)reader["created_at"]
            };
            appointments.Add(appointment);
        }
        return appointments;
    }

    // Updates a company's appointment status. Returns rows affected (0 = not found / not this company).
    public async Task<int> UpdateAppointmentStatus(Guid appointmentId, Guid companyId, AppointmentStatus status)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "UPDATE appointments SET status = @status WHERE id = @id AND company_id = @company_id";
        command.AddParameter("@status", (int)status);
        command.AddParameter("@id", appointmentId.ToString());
        command.AddParameter("@company_id", companyId.ToString());

        return await command.ExecuteNonQueryAsync();
    }

    // Dumb existence check: is any active appointment within [start, end] (BETWEEN, inclusive both ends)?
    // The policy decides the window's width (exact minute vs overlap range); the repo only asks the DB.
    public async Task<bool> SlotTaken(Guid companyId, Guid professionalId, DateTime start, DateTime end)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT EXISTS (SELECT 1 FROM appointments WHERE company_id = @company_id AND professional_id = @professional_id AND scheduled_at BETWEEN @start AND @end AND status IN (@pending, @confirmed))";
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@professional_id", professionalId.ToString());
        command.AddParameter("@start", start);
        command.AddParameter("@end", end);
        command.AddParameter("@pending", (int)AppointmentStatus.Pending);
        command.AddParameter("@confirmed", (int)AppointmentStatus.Confirmed);

        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }

    // Fetches upcoming appointments needing a reminder within [windowStart, windowEnd] that haven't been reminded yet.
    public async Task<List<UpcomingReminder>> GetPendingReminders(DateTime windowStart, DateTime windowEnd)
    {
        List<UpcomingReminder> reminders = new List<UpcomingReminder>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = @"SELECT ap.id, ap.company_id, ap.customer_name, ap.service_name, ap.scheduled_at, cv.customer_phone
                                FROM appointments ap
                                JOIN conversations cv ON ap.conversation_id = cv.id
                                WHERE ap.reminded_at IS NULL
                                  AND ap.status IN (@pending, @confirmed)
                                  AND ap.scheduled_at BETWEEN @windowStart AND @windowEnd
                                ORDER BY ap.scheduled_at";
        command.AddParameter("@pending", (int)AppointmentStatus.Pending);
        command.AddParameter("@confirmed", (int)AppointmentStatus.Confirmed);
        command.AddParameter("@windowStart", windowStart);
        command.AddParameter("@windowEnd", windowEnd);

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reminders.Add(new UpcomingReminder(
                (Guid)reader["id"],
                (Guid)reader["company_id"],
                (string)reader["customer_name"],
                (string)reader["service_name"],
                (DateTime)reader["scheduled_at"],
                (string)reader["customer_phone"]
            ));
        }
        return reminders;
    }

    // Records the timestamp when the reminder was sent to prevent duplicate messages.
    public async Task MarkReminded(Guid appointmentId, DateTime remindedAt)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "UPDATE appointments SET reminded_at = @reminded_at WHERE id = @id";
        command.AddParameter("@reminded_at", remindedAt);
        command.AddParameter("@id", appointmentId.ToString());

        await command.ExecuteNonQueryAsync();
    }
}
