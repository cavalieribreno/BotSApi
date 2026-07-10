namespace BotSaaS.Api.Shared.Security;

// Contract for password hashing -- hides the algorithm (BCrypt today).
public interface IPasswordHasher
{
   string Hash(string password);
   bool Verify(string password, string hash);
}