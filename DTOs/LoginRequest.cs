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
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    /// <example>password123</example>
    public string Password { get; set; } = string.Empty;
}
