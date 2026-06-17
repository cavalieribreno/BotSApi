using BotSaaS.Api.Shared.Security;

namespace BotSaaS.Api.Core.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }
    // Builds a valid User (owns the password hashing) and persists it.
    public async Task<User> CreateUser(string name, string email, string password, Guid companyId)
    {
        string passwordHash = _passwordHasher.Hash(password);

        User user = new User
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.InsertUser(user);
        return user;
    }
    // User lookup by email (used by login). null = not found.
    public async Task<User?> GetUserByEmail(string email)
    {
        User? user = await _userRepository.GetUserByEmail(email);
        return user;
    }
}