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

public class ProductsControllerTests : TestBase
{

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Product2", Description = "Desc2", Price = 20.0m, Stock = 10 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);

        // Act - default pagination returns paginated response
        var result = await controller.GetProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.Equal(2, paginatedResponse.TotalCount);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsPaginatedResponse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Product2", Description = "Desc2", Price = 20.0m, Stock = 10 },
            new Product { Id = 3, Name = "Product3", Description = "Desc3", Price = 30.0m, Stock = 15 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);

        // Act - with pagination parameters
        var result = await controller.GetProducts(1, 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.Equal(1, paginatedResponse.Page);
        Assert.Equal(2, paginatedResponse.PageSize);
        Assert.Equal(3, paginatedResponse.TotalCount);
        Assert.Equal(2, paginatedResponse.TotalPages);
        Assert.True(paginatedResponse.HasNextPage);
        Assert.False(paginatedResponse.HasPreviousPage);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Product 
        { 
            Id = 1, 
            Name = "TestProduct", 
            Description = "TestDesc", 
            Price = 99.99m, 
            Stock = 15 
        });
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);

        // Act
        var result = await controller.GetProduct(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("TestProduct", product.Name);
        Assert.Equal(99.99m, product.Price);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);

        // Act
        var result = await controller.GetProduct(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);
        var productDto = new ProductCreateDto
        {
            Name = "NewProduct",
            Description = "NewDesc",
            Price = 49.99m,
            Stock = 20
        };

        // Act
        var result = await controller.CreateProduct(productDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<Product>(createdResult.Value);
        Assert.Equal("NewProduct", product.Name);
        Assert.Equal(49.99m, product.Price);
        Assert.Equal(20, product.Stock);
        
        // Verify it was saved to the database
        var savedProduct = await context.Products.FindAsync(product.Id);
        Assert.NotNull(savedProduct);
        Assert.Equal("NewProduct", savedProduct.Name);
    }

    [Fact]
    public async Task UpdateProduct_WithValidId_UpdatesAndReturnsProduct()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Product 
        { 
            Id = 1, 
            Name = "Original", 
            Description = "OriginalDesc", 
            Price = 10.0m, 
            Stock = 5 
        });
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);
        var updateDto = new ProductUpdateDto
        {
            Name = "Updated",
            Price = 15.0m
        };

        // Act
        var result = await controller.UpdateProduct(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var product = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Updated", product.Name);
        Assert.Equal(15.0m, product.Price);
        Assert.Equal("OriginalDesc", product.Description); // Unchanged
        Assert.Equal(5, product.Stock); // Unchanged
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);
        var updateDto = new ProductUpdateDto { Name = "Updated" };

        // Act
        var result = await controller.UpdateProduct(999, updateDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateProduct_WithPartialData_UpdatesOnlySpecifiedFields()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Product 
        { 
            Id = 1, 
            Name = "Original", 
            Description = "OriginalDesc", 
            Price = 10.0m, 
            Stock = 5 
        });
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);
        var updateDto = new ProductUpdateDto { Stock = 25 };

        // Act
        var result = await controller.UpdateProduct(1, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var product = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Original", product.Name); // Unchanged
        Assert.Equal("OriginalDesc", product.Description); // Unchanged
        Assert.Equal(10.0m, product.Price); // Unchanged
        Assert.Equal(25, product.Stock); // Updated
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_DeletesAndReturnsSuccess()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Product 
        { 
            Id = 1, 
            Name = "ToDelete", 
            Description = "Desc", 
            Price = 10.0m, 
            Stock = 5 
        });
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);

        // Act
        var result = await controller.DeleteProduct(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        
        // Verify it was deleted from the database
        var deletedProduct = await context.Products.FindAsync(1);
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = Substitute.For<ILogger<ProductsController>>();
        var controller = new ProductsController(context, logger);

        // Act
        var result = await controller.DeleteProduct(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
