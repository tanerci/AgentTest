using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Controllers;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Resources;
using System.Security.Claims;
using Xunit;

namespace ProductApi.Tests;

public class AuthControllerTests : TestBase
{
    private AuthController CreateControllerWithMockHttpContext(AppDbContext context)
    {
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(context, logger, localizer);
        
        // Create mock HttpContext
        var httpContext = Substitute.For<HttpContext>();
        var authService = Substitute.For<IAuthenticationService>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authService);
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
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(context, logger, localizer);
        
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
        
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(context, logger, localizer);
        
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
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(context, logger, localizer);
        
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
        var logger = Substitute.For<ILogger<AuthController>>();
        var localizer = GetMockLocalizer();
        var controller = new AuthController(context, logger, localizer);
        
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
