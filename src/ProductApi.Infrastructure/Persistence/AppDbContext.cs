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

        // Seed a test user (password: "password123")
        modelBuilder.Entity<User>().HasData(
            new User 
            { 
                Id = 1, 
                Username = "admin", 
                PasswordHash = "$2a$11$xZ1Y2Z3A4B5C6D7E8F9G0H1I2J3K4L5M6N7O8P9Q0R1S2T3U4V5W6X" // BCrypt hash
            }
        );
    }
}
