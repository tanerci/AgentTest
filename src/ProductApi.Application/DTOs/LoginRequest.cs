using System.ComponentModel.DataAnnotations;

namespace ProductApi.Application.DTOs;

/// <summary>
/// DTO for login requests.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username for authentication.
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The password for authentication.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
