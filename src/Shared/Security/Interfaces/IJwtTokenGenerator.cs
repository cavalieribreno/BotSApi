namespace BotSaaS.Api.Shared.Security;

// Contract for issuing the login JWT.
public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, Guid companyId, string email);
}