using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Users;

[Authorize]
[ApiController]
[Route("api")]
public class UserController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
    {
        string? userId = User.FindFirstValue("sub");
        string? companyId = User.FindFirstValue("companyId");
        string? email = User.FindFirstValue("email");

        return Ok( new { userId, companyId, email } );
    }
}