using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Controllers;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;
using Xunit;

namespace ProductApi.Tests;

/// <summary>
/// Tests for error handling and logging in controllers.
/// </summary>
public class ErrorHandlingTests : TestBase
{
    [Fact]
    public async Task GetProduct_DatabaseException_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = GetMockLogger<ProductsController>();
        var controller = new ProductsController(context, logger);
        
        // Force an exception by disposing the context
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await controller.GetProduct(1);
        });
    }

    [Fact]
    public async Task CreateProduct_DatabaseException_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = GetMockLogger<ProductsController>();
        var controller = new ProductsController(context, logger);
        var productDto = new ProductCreateDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };
        
        // Force an exception by disposing the context
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await controller.CreateProduct(productDto);
        });
    }

    [Fact]
    public async Task UpdateProduct_DatabaseException_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Product
        {
            Id = 1,
            Name = "Original",
            Description = "Original Description",
            Price = 10.0m,
            Stock = 5
        });
        await context.SaveChangesAsync();
        
        var logger = GetMockLogger<ProductsController>();
        var controller = new ProductsController(context, logger);
        var updateDto = new ProductUpdateDto
        {
            Name = "Updated"
        };
        
        // Force an exception by disposing the context
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await controller.UpdateProduct(1, updateDto);
        });
    }

    [Fact]
    public async Task DeleteProduct_DatabaseException_ThrowsException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Product
        {
            Id = 1,
            Name = "To Delete",
            Description = "Description",
            Price = 10.0m,
            Stock = 5
        });
        await context.SaveChangesAsync();
        
        var logger = GetMockLogger<ProductsController>();
        var controller = new ProductsController(context, logger);
        
        // Force an exception by disposing the context
        await context.DisposeAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await controller.DeleteProduct(1);
        });
    }

    // Note: AuthController tests require HttpContext setup which is handled by integration tests
    // Unit tests for database exceptions in AuthController are omitted here

    [Fact]
    public async Task GetProducts_LogsInformation_OnSuccess()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();
        
        var logger = GetMockLogger<ProductsController>();
        var controller = new ProductsController(context, logger);

        // Act
        var result = await controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        
        // Verify that logger was called (using NSubstitute verification)
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully retrieved")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task CreateProduct_LogsInformation_OnSuccess()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = GetMockLogger<ProductsController>();
        var controller = new ProductsController(context, logger);
        var productDto = new ProductCreateDto
        {
            Name = "New Product",
            Description = "New Description",
            Price = 49.99m,
            Stock = 20
        };

        // Act
        var result = await controller.CreateProduct(productDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        
        // Verify that logger was called for successful creation
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully created product")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
