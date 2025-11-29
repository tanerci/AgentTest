using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ProductApi.Application.Services;
using ProductApi.DTOs;
using ProductApi.Resources;
using System.Security.Claims;

namespace ProductApi.Controllers;

/// <summary>
/// Handles user authentication operations including login, logout, and status checks.
/// Thin controller that delegates to the AuthService following DDD principles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the AuthController.
    /// </summary>
    /// <param name="authService">The authentication application service.</param>
    /// <param name="logger">The logger for security event tracking.</param>
    /// <param name="localizer">The string localizer for localized messages.</param>
    public AuthController(IAuthService authService, ILogger<AuthController> logger, IStringLocalizer<SharedResource> localizer)
    {
        _authService = authService;
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

        var result = await _authService.AuthenticateAsync(
            request.Username, 
            request.Password, 
            GetClientIpAddress());

        if (result.IsFailure)
        {
            return Unauthorized(new { message = _localizer["InvalidUsernameOrPassword"].Value });
        }

        var authResult = result.Value;
        var claimsIdentity = new ClaimsIdentity(authResult.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return Ok(new { message = _localizer["LoginSuccessful"].Value, username = authResult.Username });
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
