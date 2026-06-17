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