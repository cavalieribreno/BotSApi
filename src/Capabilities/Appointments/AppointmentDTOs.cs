namespace BotSaaS.Api.Capabilities.Appointments;

// Owner-facing view of an appointment. Status is the enum name (frontend localizes).
public record AppointmentResponse(
    Guid Id,
    Guid CompanyId,
    Guid ConversationId,
    string ServiceName,
    string CustomerName,
    DateTime ScheduledAt,
    string Status,
    DateTime CreatedAt);

// Deserialized args of the registrar_agendamento tool call (the model's JSON, mapped to the domain).
// Fields in PT on purpose - they're the JSON keys the model fills (servico/data/hora/nome).
public record AppointmentArgs(string Servico, string Data, string Hora, string Nome);
