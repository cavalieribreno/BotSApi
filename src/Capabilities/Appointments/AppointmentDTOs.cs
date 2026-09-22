namespace BotSaaS.Api.Capabilities.Appointments;

// Owner-facing view of an appointment. Status is the enum name (frontend localizes).
public record AppointmentResponse(
    Guid Id,
    Guid CompanyId,
    Guid ConversationId,
    Guid ProfessionalId,
    string ProfessionalName,
    string ServiceName,
    string CustomerName,
    DateTime ScheduledAt,
    string Status,
    DateTime CreatedAt);

// Deserialized args of the registrar_agendamento tool call (the model's JSON, mapped to the domain).
// Fields in PT on purpose - they're the JSON keys the model fills (servico/data/hora/nome/profissional).
public record AppointmentArgs(string Servico, string Data, string Hora, string Nome, string Profissional);

// DTO carrying appointment details + contact phone from JOIN with conversations, used by the reminder worker.
public record UpcomingReminder(
    Guid AppointmentId,
    Guid CompanyId,
    string CustomerName,
    string ServiceName,
    DateTime ScheduledAt,
    string CustomerPhone);
