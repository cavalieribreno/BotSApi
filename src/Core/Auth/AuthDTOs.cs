using System.ComponentModel.DataAnnotations;
using BotSaaS.Api.Core.Companies;

namespace BotSaaS.Api.Core.Auth;

// DataAnnotations = border validation (presence/format/length); business rules stay in the services.
// Entry dto for Register Company and User
public class RegisterRequest
{
    [Required]
    [MaxLength(150)]
    public string OwnerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    public Segment Segment { get; set; }
}

// User response for Registration response
public class UserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// login request dto
public class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}