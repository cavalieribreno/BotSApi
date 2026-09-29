namespace BotSaaS.Api.Core.Customers;

// Customer DTOs for HTTP API and UI consumption.
public record CustomerResponse(Guid Id, string Name, string Phone, DateTime CreatedAt);
public record CreateCustomerRequest(string Name, string Phone);
