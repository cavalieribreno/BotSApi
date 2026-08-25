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
        command.CommandText = "INSERT INTO appointments (id, company_id, conversation_id, service_name, customer_name, scheduled_at, status, created_at) VALUES (@id, @company_id, @conversation_id, @service_name, @customer_name, @scheduled_at, @status, @created_at)";
        command.AddParameter("@id", appointment.Id.ToString());
        command.AddParameter("@company_id", appointment.CompanyId.ToString());       // FK, guid
        command.AddParameter("@conversation_id", appointment.ConversationId.ToString()); // FK, guid
        command.AddParameter("@service_name", appointment.ServiceName);
        command.AddParameter("@customer_name", appointment.CustomerName);
        command.AddParameter("@scheduled_at", appointment.ScheduledAt);
        command.AddParameter("@status", (int)appointment.Status);
        command.AddParameter("@created_at", appointment.CreatedAt);

        await command.ExecuteNonQueryAsync();
    }

    // All appointments of a company, soonest-first. List empty, never null.
    public async Task<List<Appointment>> GetAppointmentsByCompany(Guid companyId)
    {
        List<Appointment> appointments = new List<Appointment>();

        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT id, company_id, conversation_id, service_name, customer_name, scheduled_at, status, created_at FROM appointments WHERE company_id = @company_id ORDER BY scheduled_at";
        command.AddParameter("@company_id", companyId.ToString());

        using DbDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Appointment appointment = new Appointment
            {
                Id = (Guid)reader["id"],
                CompanyId = (Guid)reader["company_id"],
                ConversationId = (Guid)reader["conversation_id"],
                ServiceName = (string)reader["service_name"],
                CustomerName = (string)reader["customer_name"],
                ScheduledAt = (DateTime)reader["scheduled_at"],
                Status = (AppointmentStatus)(int)reader["status"],
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
    public async Task<bool> SlotTaken(Guid companyId, DateTime start, DateTime end)
    {
        using DbCommand command = _dbSession.Connection.CreateCommand();
        command.Transaction = _dbSession.Transaction;
        command.CommandText = "SELECT EXISTS (SELECT 1 FROM appointments WHERE company_id = @company_id AND scheduled_at BETWEEN @start AND @end AND status IN (@pending, @confirmed))";
        command.AddParameter("@company_id", companyId.ToString());
        command.AddParameter("@start", start);
        command.AddParameter("@end", end);
        command.AddParameter("@pending", (int)AppointmentStatus.Pending);
        command.AddParameter("@confirmed", (int)AppointmentStatus.Confirmed);

        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }
}
