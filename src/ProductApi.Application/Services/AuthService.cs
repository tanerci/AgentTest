using System.Security.Claims;
using Microsoft.Extensions.Logging;
using ProductApi.Application.Common;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;

namespace ProductApi.Application.Services;

/// <summary>
/// Application service for authentication-related business operations.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<AuthResult>> AuthenticateAsync(
        string username, 
        string password, 
        string clientIpAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user == null || !user.VerifyPassword(password))
        {
            _logger.LogWarning("Failed login attempt for username: {Username} from IP: {IpAddress}", 
                username, clientIpAddress);
            return Result.Failure<AuthResult>(Error.Unauthorized("Invalid username or password"));
        }

        _logger.LogInformation("Successful login for user: {Username} from IP: {IpAddress}", 
            user.Username, clientIpAddress);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        return Result.Success(new AuthResult
        {
            UserId = user.Id,
            Username = user.Username,
            Claims = claims
        });
    }

    public async Task<Result<UserEntity>> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserEntity>(Error.NotFound($"User with ID {id} not found"));
        }

        return Result.Success(user);
    }
}

/// <summary>
/// Result of a successful authentication.
/// </summary>
public class AuthResult
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public IList<Claim> Claims { get; set; } = new List<Claim>();
}
