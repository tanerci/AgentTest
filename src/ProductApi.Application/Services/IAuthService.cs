using ProductApi.Application.Common;
using ProductApi.Domain.Entities;

namespace ProductApi.Application.Services;

/// <summary>
/// Interface for authentication-related application services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    Task<Result<AuthResult>> AuthenticateAsync(
        string username, 
        string password, 
        string clientIpAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    Task<Result<UserEntity>> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
}
