namespace ProductApi.Domain.Entities;

/// <summary>
/// Domain entity representing a user account for authentication.
/// Encapsulates authentication-related business rules.
/// </summary>
public class UserEntity
{
    public int Id { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }

    // Required for EF Core
    private UserEntity()
    {
        Username = string.Empty;
        PasswordHash = string.Empty;
    }

    private UserEntity(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// Factory method to create a new User entity with hashed password.
    /// </summary>
    public static UserEntity Create(string username, string passwordHash)
    {
        ValidateUsername(username);
        
        return new UserEntity(username, passwordHash);
    }

    /// <summary>
    /// Verifies if the provided password matches the stored hash.
    /// </summary>
    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }

    /// <summary>
    /// Updates the user's password.
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    private static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (username.Length < 3 || username.Length > 50)
            throw new ArgumentException("Username must be between 3 and 50 characters", nameof(username));
    }
}
