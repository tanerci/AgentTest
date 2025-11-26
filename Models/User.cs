namespace ProductApi.Models;

/// <summary>
/// Represents a user account for authentication.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the username for login.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the bcrypt hashed password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
}
