namespace BotSaaS.Api.Core.Users;

// Contract for user data access -- talks SQL.
public interface IUserRepository
{
    public Task InsertUser(User user);
    public Task<User?> GetUserByEmail(string email);
}