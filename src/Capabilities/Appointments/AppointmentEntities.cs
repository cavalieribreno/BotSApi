using System.Text.Json.Serialization;

namespace BotSaaS.Api.Capabilities.Appointments;

// Origin of the appointment booking.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppointmentOrigin
{
    Bot = 0,
    Manual = 1
}

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

// Appointment: a structured booking extracted from a conversation or created by the reception desk.
public class Appointment
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }          // tenant
    public Guid? ConversationId { get; set; }    // optional: populated when booked via chat bot
    public Guid ProfessionalId { get; set; }     // assigned professional
    public string ProfessionalName { get; set; } = string.Empty; // populated via JOIN with professionals
    public string ServiceName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }    // the appointment moment
    public AppointmentStatus Status { get; set; } // defaults to Pending (0)
    public AppointmentOrigin Origin { get; set; } // defaults to Bot (0)
    public DateTime? RemindedAt { get; set; }    // when the reminder was sent (null if not yet)
    public DateTime CreatedAt { get; set; }      // when booked (app-filled)
}
