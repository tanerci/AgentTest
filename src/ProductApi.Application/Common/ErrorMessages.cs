namespace ProductApi.Application.Common;

/// <summary>
/// Centralized error messages for consistent error handling across the application.
/// Using constants ensures DRY principle and consistent error messages.
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// Error message format for product not found.
    /// </summary>
    public const string ProductNotFoundFormat = "Product with ID {0} not found";

    /// <summary>
    /// Error message format for user not found.
    /// </summary>
    public const string UserNotFoundFormat = "User with ID {0} not found";

    /// <summary>
    /// Error message for invalid credentials.
    /// </summary>
    public const string InvalidCredentials = "Invalid username or password";

    /// <summary>
    /// Gets a formatted product not found error message.
    /// </summary>
    public static string ProductNotFound(int id) => $"Product with ID {id} not found";

    /// <summary>
    /// Gets a formatted user not found error message.
    /// </summary>
    public static string UserNotFound(int id) => $"User with ID {id} not found";
}
