using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Resources;
using System.Security.Claims;

namespace ProductApi.Controllers;

/// <summary>
/// Handles user authentication operations including login, logout, and status checks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the AuthController.
    /// </summary>
    /// <param name="context">The database context for user operations.</param>
    /// <param name="logger">The logger for security event tracking.</param>
    /// <param name="localizer">The string localizer for localized messages.</param>
    public AuthController(AppDbContext context, ILogger<AuthController> logger, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _logger = logger;
        _localizer = localizer;
    }

    /// <summary>
    /// Gets the real client IP address, checking X-Forwarded-For header if behind a proxy.
    /// </summary>
    private string GetClientIpAddress()
    {
        // Check X-Forwarded-For header first (for proxy/load balancer scenarios)
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one (original client)
            return forwardedFor.Split(',')[0].Trim();
        }

        // Fallback to direct connection IP
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Authenticates a user and creates an authentication cookie.
    /// </summary>
    /// <param name="request">The login credentials containing username and password.</param>
    /// <returns>Success message with username on successful authentication, or unauthorized error.</returns>
    /// <response code="200">Login successful - authentication cookie created.</response>
    /// <response code="400">Invalid request - validation errors.</response>
    /// <response code="401">Invalid username or password.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///        "username": "admin",
    ///        "password": "password123"
    ///     }
    ///     
    /// The authentication cookie will be valid for 24 hours.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login attempt with invalid model state from IP: {IpAddress}", GetClientIpAddress());
            return BadRequest(ModelState);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username} from IP: {IpAddress}", 
                request.Username, GetClientIpAddress());
            return Unauthorized(new { message = _localizer["InvalidUsernameOrPassword"].Value });
        }

        _logger.LogInformation("Successful login for user: {Username} from IP: {IpAddress}", 
            user.Username, GetClientIpAddress());

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok(new { message = _localizer["LoginSuccessful"].Value, username = user.Username });
    }

    /// <summary>
    /// Logs out the current user by clearing the authentication cookie.
    /// </summary>
    /// <returns>Success message confirming logout.</returns>
    /// <response code="200">Logout successful - authentication cookie cleared.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/logout
    ///     
    /// This endpoint clears the authentication cookie, logging out the user.
    /// </remarks>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name ?? "Unknown";
        _logger.LogInformation("User logged out: {Username}", username);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = _localizer["LogoutSuccessful"].Value });
    }

    /// <summary>
    /// Checks the current authentication status of the user.
    /// </summary>
    /// <returns>Object containing authentication status and username if authenticated.</returns>
    /// <response code="200">Returns authentication status and username if logged in.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/auth/status
    ///     
    /// Returns { "authenticated": true, "username": "admin" } if logged in,
    /// or { "authenticated": false } if not logged in.
    /// </remarks>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Status()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok(new { authenticated = true, username = User.Identity.Name });
        }
        return Ok(new { authenticated = false });
    }
}
