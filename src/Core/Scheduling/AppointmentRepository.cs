using System.Data.Common;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Core.Scheduling;

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
}
