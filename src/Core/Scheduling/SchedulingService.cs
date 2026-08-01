using System.Globalization;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Scheduling;

// Owns "what a valid Appointment is": parses date+time, builds and persists it.
public class SchedulingService : ISchedulingService
{
    private readonly IAppointmentRepository _appointmentRepository;
    public SchedulingService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    // Builds a valid Appointment from primitives (parses "yyyy-MM-dd" + "HH:mm" into ScheduledAt, business-local naive time) and persists it.
    public async Task<Result<Appointment>> CreateAppointment(Guid companyId, Guid conversationId, string serviceName, string customerName, string data, string hora)
    {
        // strict parse - if the model sent a bad date/time, fail cleanly instead of crashing
        if (!DateTime.TryParseExact($"{data} {hora}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime scheduledAt))
        {
            return Result<Appointment>.Failure("Data ou hora inválida");
        }

        Appointment appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ConversationId = conversationId,
            ServiceName = serviceName,
            CustomerName = customerName,
            ScheduledAt = scheduledAt,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _appointmentRepository.InsertAppointment(appointment);
        return Result<Appointment>.Success(appointment);
    }
}
