namespace BotSaaS.Api.Core.Companies;

// Request payload when updating a day's operating hours (hours as "HH:mm" strings, e.g. "09:00")
public record BusinessHoursRequest(
    int DayOfWeek,
    string OpensAt,
    string ClosesAt,
    bool IsClosed);

// Response payload returned to the frontend
public record BusinessHoursResponse(
    int DayOfWeek,
    string OpensAt,
    string ClosesAt,
    bool IsClosed);
