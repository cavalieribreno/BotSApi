namespace BotSaaS.Api.Core.Users;

public interface IUserRepository
{
    public Task InsertUser(User user);
    public Task<User?> GetUserByEmail(string email);
}