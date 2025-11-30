using Microsoft.EntityFrameworkCore;
using ProductApi.Infrastructure.Persistence.Models;

namespace ProductApi.Infrastructure.Persistence;

/// <summary>
/// Database context for the Product API application.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the AppDbContext.
    /// </summary>
    /// <param name="options">The options for configuring the database context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Products entity set.
    /// </summary>
    public DbSet<Product> Products { get; set; }
    
    /// <summary>
    /// Gets or sets the Users entity set.
    /// </summary>
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed some initial products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 10 },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Stock = 50 },
            new Product { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard", Price = 89.99m, Stock = 25 }
        );

        // WARNING: This seed data is for development/testing only.
        // In production:
        // - Use environment-specific initialization scripts or admin setup endpoints
        // - Never commit real credentials to source control
        // - Use secure password policies and unique passwords per environment
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "admin", 
                PasswordHash = "$2a$11$SDCYH0wn9mp7m7g7CxakUerO/pOj.UspJlNDckQujniUDsBMtSRky" // BCrypt hash for test password "password123"
            }
        );
    }
}
