using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Application.DTOs;
using ProductApi.Controllers;
using ProductApi.Infrastructure.Persistence;
using ProductApi.Infrastructure.Persistence.Models;
using System.Security.Claims;
using Xunit;

namespace ProductApi.Tests;

public class AuthControllerTests : TestBase
{
    private AuthController CreateControllerWithMockHttpContext(AppDbContext context)
    {
        var authService = GetAuthService(context);
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(authService, logger, localizer);
        
        // Create mock HttpContext
        var httpContext = Substitute.For<HttpContext>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authenticationService);
        httpContext.RequestServices.Returns(serviceProvider);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        return controller;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var context = GetInMemoryDbContext();
        context.Users.Add(new User 
        { 
            Id = 1, 
            Username = "testuser", 
            PasswordHash = passwordHash 
        });
        await context.SaveChangesAsync();
        
        var controller = CreateControllerWithMockHttpContext(context);
        var loginRequest = new LoginRequest 
        { 
            Username = "testuser", 
            Password = "password123" 
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var controller = CreateControllerWithMockHttpContext(context);
        
        // Simulate invalid model state (e.g., missing required fields)
        controller.ModelState.AddModelError("Username", "Username is required");
        controller.ModelState.AddModelError("Password", "Password is required");
        
        var loginRequest = new LoginRequest 
        { 
            Username = "", 
            Password = "" 
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Login_WithXForwardedForHeader_UsesCorrectIpAddress()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var context = GetInMemoryDbContext();
        context.Users.Add(new User 
        { 
            Id = 1, 
            Username = "testuser", 
            PasswordHash = passwordHash 
        });
        await context.SaveChangesAsync();
        
        var authService = GetAuthService(context);
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(authService, logger, localizer);

        // Create mock HttpContext with X-Forwarded-For header
        var httpContext = Substitute.For<HttpContext>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary { { "X-Forwarded-For", "192.168.1.1, 10.0.0.1" } };
        
        request.Headers.Returns(headers);
        httpContext.Request.Returns(request);
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authenticationService);
        httpContext.RequestServices.Returns(serviceProvider);
        httpContext.Connection.RemoteIpAddress.Returns((System.Net.IPAddress?)null);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var loginRequest = new LoginRequest 
        { 
            Username = "testuser", 
            Password = "password123" 
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_WithoutXForwardedFor_UsesRemoteIpAddress()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var context = GetInMemoryDbContext();
        context.Users.Add(new User 
        { 
            Id = 1, 
            Username = "testuser", 
            PasswordHash = passwordHash 
        });
        await context.SaveChangesAsync();
        
        var authService = GetAuthService(context);
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(authService, logger, localizer);

        // Create mock HttpContext without X-Forwarded-For header
        var httpContext = Substitute.For<HttpContext>();
        var authenticationService = Substitute.For<IAuthenticationService>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary();
        var connection = Substitute.For<ConnectionInfo>();
        
        request.Headers.Returns(headers);
        httpContext.Request.Returns(request);
        connection.RemoteIpAddress.Returns(System.Net.IPAddress.Parse("127.0.0.1"));
        httpContext.Connection.Returns(connection);
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authenticationService);
        httpContext.RequestServices.Returns(serviceProvider);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var loginRequest = new LoginRequest 
        { 
            Username = "testuser", 
            Password = "password123" 
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_WithNullRemoteIp_UsesUnknownIpAddress()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var context = GetInMemoryDbContext();
        context.Users.Add(new User 
        { 
            Id = 1, 
            Username = "testuser", 
            PasswordHash = passwordHash 
        });
        await context.SaveChangesAsync();
        
        var controller = CreateControllerWithMockHttpContext(context);
        
        var loginRequest = new LoginRequest 
        { 
            Username = "testuser", 
            Password = "password123" 
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var authService = GetAuthService(context);
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(authService, logger, localizer);
        
        // Add HttpContext mock
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var loginRequest = new LoginRequest 
        { 
            Username = "nonexistent", 
            Password = "password123" 
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        context.Users.Add(new User 
        { 
            Id = 1, 
            Username = "testuser", 
            PasswordHash = passwordHash 
        });
        await context.SaveChangesAsync();
        
        var authService = GetAuthService(context);
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(authService, logger, localizer);
        
        // Add HttpContext mock
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        var loginRequest = new LoginRequest 
        { 
            Username = "testuser", 
            Password = "wrongpassword" 
        };

        // Act
        var result = await controller.Login(loginRequest);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public async Task Logout_ReturnsSuccess()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var controller = CreateControllerWithMockHttpContext(context);
        
        // Act
        var result = await controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void Status_WhenAuthenticated_ReturnsAuthenticatedTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var authService = GetAuthService(context);
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(authService, logger, localizer);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = controller.Status();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value);
        
        // Use reflection to check properties
        var authenticatedProp = value.GetType().GetProperty("authenticated");
        var usernameProp = value.GetType().GetProperty("username");
        
        Assert.NotNull(authenticatedProp);
        Assert.NotNull(usernameProp);
        Assert.True((bool)authenticatedProp.GetValue(value)!);
        Assert.Equal("testuser", usernameProp.GetValue(value));
    }

    [Fact]
    public void Status_WhenNotAuthenticated_ReturnsAuthenticatedFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var authService = GetAuthService(context);
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(authService, logger, localizer);
        
        var httpContext = new DefaultHttpContext();
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = controller.Status();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = okResult.Value;
        Assert.NotNull(value);
        
        // Use reflection to check properties
        var authenticatedProp = value.GetType().GetProperty("authenticated");
        Assert.NotNull(authenticatedProp);
        Assert.False((bool)authenticatedProp.GetValue(value)!);
    }
}
