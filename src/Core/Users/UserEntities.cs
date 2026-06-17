namespace BotSaaS.Api.Core.Users;

// Owner of company, who will login -- (Not the final client)

public class User
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; } // company of the user 
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}