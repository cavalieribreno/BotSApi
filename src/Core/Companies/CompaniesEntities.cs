namespace BotSaaS.Api.Core.Companies;

// Enum for segment of company
public enum Segment
{
    Generic = 0
}

// Company entity
public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Segment Segment { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Operating hours per day of week for a company.
public class BusinessHours
{
    public Guid CompanyId { get; set; }
    public DayOfWeek DayOfWeek { get; set; } // 0 = Sunday ... 6 = Saturday (native C# enum)
    public TimeSpan OpensAt { get; set; }
    public TimeSpan ClosesAt { get; set; }
    public bool IsClosed { get; set; }
}