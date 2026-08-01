namespace BotSaaS.Api.Core.Scheduling;

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
