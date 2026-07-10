namespace BotSaaS.Api.Shared.Security;

// BCrypt implementation of IPasswordHasher; the salt is embedded in the generated hash.
public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}