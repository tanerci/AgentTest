using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Tests;

public abstract class TestBase
{
    protected AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new AppDbContext(options);
    }
}
