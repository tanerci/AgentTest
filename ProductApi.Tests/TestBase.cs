using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

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
}
