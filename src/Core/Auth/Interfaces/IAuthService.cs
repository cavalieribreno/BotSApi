using BotSaaS.Api.Core.Users;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Auth;

// Contract for authentication: register (Company + User) and login.
public interface IAuthService
{
    Task<Result<User>> Register(RegisterRequest request);
    public Task<Result<string>> Login(LoginRequest request);
}