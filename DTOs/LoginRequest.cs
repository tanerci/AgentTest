using System.ComponentModel.DataAnnotations;

namespace ProductApi.DTOs;

/// <summary>
/// Request model for user authentication.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    /// <example>password123</example>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;
}
