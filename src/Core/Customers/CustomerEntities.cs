namespace BotSaaS.Api.Core.Customers;

// Aggregate root representing a customer of a company tenant.
public class Customer
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
