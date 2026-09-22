using System.Text.Json.Serialization;

namespace BotSaaS.Api.Capabilities.Appointments;

// Appointment lifecycle. Explicit values -- persisted as INT, never reorder.
// Serialized as string names ("Pending", "Confirmed", etc.) over HTTP JSON.
[JsonConverter(typeof(JsonStringEnumConverter))]
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
    public Guid ProfessionalId { get; set; }     // assigned professional
    public string ProfessionalName { get; set; } = string.Empty; // populated via JOIN with professionals
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }    // the appointment moment
    public AppointmentStatus Status { get; set; } // defaults to Pending (0)
    public DateTime? RemindedAt { get; set; }    // when the reminder was sent (null if not yet)
    public DateTime CreatedAt { get; set; }      // when booked (app-filled)
}
