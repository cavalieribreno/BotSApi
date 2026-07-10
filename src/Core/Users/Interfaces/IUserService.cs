namespace BotSaaS.Api.Core.Users;

// Contract for user domain operations.
public interface IUserService
{
    public Task<User> CreateUser(string name, string email, string password, Guid companyId);
    public Task<User?> GetUserByEmail(string email);
}