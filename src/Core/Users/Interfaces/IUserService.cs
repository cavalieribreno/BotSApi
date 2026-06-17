namespace BotSaaS.Api.Core.Users;

public interface IUserService
{
    public Task<User> CreateUser(string name, string email, string password, Guid companyId);
    public Task<User?> GetUserByEmail(string email);
}