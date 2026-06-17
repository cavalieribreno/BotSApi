namespace BotSaaS.Api.Shared.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, Guid companyId, string email);
}