using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Application.Services;
using ProductApi.Data;
using ProductApi.Domain.Repositories;
using ProductApi.Infrastructure.Repositories;
using ProductApi.Resources;

namespace ProductApi.Tests;

/// <summary>
/// Base class for all test classes providing common test utilities.
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// Creates a new isolated in-memory database context for testing.
    /// Each call creates a fresh database with a unique name to ensure test isolation.
    /// </summary>
    /// <returns>A new AppDbContext instance configured with an in-memory database.</returns>
    protected AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new AppDbContext(options);
    }

    /// <summary>
    /// Creates a mock string localizer for testing localized strings.
    /// Returns the key as the value for any localization lookup.
    /// </summary>
    /// <returns>A mock IStringLocalizer&lt;SharedResource&gt; instance.</returns>
    protected IStringLocalizer<SharedResource> GetMockLocalizer()
    {
        var localizer = Substitute.For<IStringLocalizer<SharedResource>>();
        
        localizer["InvalidUsernameOrPassword"].Returns(new LocalizedString("InvalidUsernameOrPassword", "Invalid username or password"));
        localizer["LoginSuccessful"].Returns(new LocalizedString("LoginSuccessful", "Login successful"));
        localizer["LogoutSuccessful"].Returns(new LocalizedString("LogoutSuccessful", "Logout successful"));
        localizer["ProductNotFound"].Returns(new LocalizedString("ProductNotFound", "Product with ID {0} not found"));
        localizer["ProductDeletedSuccessfully"].Returns(new LocalizedString("ProductDeletedSuccessfully", "Product deleted successfully"));
        
        return localizer;
    }

    /// <summary>
    /// Creates a ProductService with the given context for testing.
    /// </summary>
    protected IProductService GetProductService(AppDbContext context)
    {
        var repository = new ProductRepository(context);
        var logger = Substitute.For<ILogger<ProductService>>();
        return new ProductService(repository, logger);
    }

    /// <summary>
    /// Creates an AuthService with the given context for testing.
    /// </summary>
    protected IAuthService GetAuthService(AppDbContext context)
    {
        var repository = new UserRepository(context);
        var logger = Substitute.For<ILogger<AuthService>>();
        return new AuthService(repository, logger);
    }
}
