using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BotSaaS.Api.Shared.Security;

// Builds and signs the JWT issued at login.
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpiresHours;

    public JwtTokenGenerator()
    {
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET não definido");
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new InvalidOperationException("JWT_ISSUER não definido");
        _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new InvalidOperationException("JWT_AUDIENCE não definido");
        _jwtExpiresHours = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRES_HOURS")
            ?? throw new InvalidOperationException("JWT_EXPIRES_HOURS não definido"));                              
    }

    public string GenerateToken(Guid userId, Guid companyId, string email)
    {
        // claims = data carried inside the token (NEVER the password). companyId isolates the tenant.
        Claim[] claims =
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("companyId", companyId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email)
        };

        // sign with the secret (HMAC-SHA256) -- this is what prevents forging the token
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtExpiresHours),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}