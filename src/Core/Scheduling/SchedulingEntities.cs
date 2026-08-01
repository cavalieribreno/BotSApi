namespace BotSaaS.Api.Core.Scheduling;

// Appointment lifecycle. Explicit values -- persisted as INT, never reorder.
public enum AppointmentStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Completed = 3
}

// Appointment: a structured booking extracted from a conversation (the transactional record the owner sees).
public class Appointment
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }          // tenant
    public Guid ConversationId { get; set; }     // which conversation produced it
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }    // the appointment moment
    public AppointmentStatus Status { get; set; } // defaults to Pending (0)
    public DateTime CreatedAt { get; set; }      // when booked (app-filled)
}
