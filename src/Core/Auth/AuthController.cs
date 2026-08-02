using BotSaaS.Api.Core.Users;
using BotSaaS.Api.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        Result<User> result = await _authService.Register(request);

        if(!result.IsSuccess) return BadRequest(new { error = result.Error });

        // map to UserResponse -- never return the User raw (it carries the password hash)
        User user = result.Value!;
        UserResponse response = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
        return Ok(response);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        Result<LoginResult> result = await _authService.Login(request);
        if(!result.IsSuccess) return Unauthorized( new { error = result.Error });

        LoginResult login = result.Value!;
        return Ok(new { token = login.Token, companyName = login.CompanyName });
    }
}