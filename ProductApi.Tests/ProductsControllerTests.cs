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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);
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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);
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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);
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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);
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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

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
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.DeleteProduct(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #region FilterProducts Tests

    [Fact]
    public async Task FilterProducts_WithNoFilters_ReturnsAllProducts()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Alpha Product", Description = "Desc1", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Beta Product", Description = "Desc2", Price = 20.0m, Stock = 10 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.Equal(2, paginatedResponse.TotalCount);
    }

    [Fact]
    public async Task FilterProducts_WithSearchTerm_MatchesProductName()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Wireless Mouse", Description = "Computer accessory", Price = 25.0m, Stock = 50 },
            new Product { Id = 2, Name = "USB Keyboard", Description = "Input device", Price = 45.0m, Stock = 30 },
            new Product { Id = 3, Name = "Gaming Mouse", Description = "RGB gaming accessory", Price = 75.0m, Stock = 20 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Search for "Mouse" in name
        var result = await controller.FilterProducts(searchTerm: "Mouse");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.All(paginatedResponse.Items, p => Assert.Contains("Mouse", p.Name));
    }

    [Fact]
    public async Task FilterProducts_WithSearchTerm_MatchesProductDescription()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Laptop Stand", Description = "Ergonomic laptop accessory", Price = 35.0m, Stock = 40 },
            new Product { Id = 2, Name = "Monitor Arm", Description = "Desk mount for displays", Price = 55.0m, Stock = 25 },
            new Product { Id = 3, Name = "Desk Mat", Description = "Large ergonomic mouse pad", Price = 20.0m, Stock = 60 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Search for "ergonomic" in description
        var result = await controller.FilterProducts(searchTerm: "ergonomic");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
    }

    [Fact]
    public async Task FilterProducts_WithSearchTerm_IsCaseInsensitive()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "WIRELESS MOUSE", Description = "Upper case name", Price = 25.0m, Stock = 50 },
            new Product { Id = 2, Name = "wireless keyboard", Description = "Lower case name", Price = 45.0m, Stock = 30 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Search with mixed case
        var result = await controller.FilterProducts(searchTerm: "WiReLeSs");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
    }

    [Fact]
    public async Task FilterProducts_WithMinPrice_ReturnsProductsAboveMinPrice()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Budget Item", Description = "Cheap product", Price = 10.0m, Stock = 100 },
            new Product { Id = 2, Name = "Mid Range", Description = "Average price", Price = 50.0m, Stock = 50 },
            new Product { Id = 3, Name = "Premium Item", Description = "Expensive product", Price = 100.0m, Stock = 20 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(minPrice: 50.0m);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.All(paginatedResponse.Items, p => Assert.True(p.Price >= 50.0m));
    }

    [Fact]
    public async Task FilterProducts_WithMaxPrice_ReturnsProductsBelowMaxPrice()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Budget Item", Description = "Cheap product", Price = 10.0m, Stock = 100 },
            new Product { Id = 2, Name = "Mid Range", Description = "Average price", Price = 50.0m, Stock = 50 },
            new Product { Id = 3, Name = "Premium Item", Description = "Expensive product", Price = 100.0m, Stock = 20 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(maxPrice: 50.0m);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.All(paginatedResponse.Items, p => Assert.True(p.Price <= 50.0m));
    }

    [Fact]
    public async Task FilterProducts_WithPriceRange_ReturnsProductsWithinRange()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Very Cheap", Description = "Desc", Price = 5.0m, Stock = 100 },
            new Product { Id = 2, Name = "Budget", Description = "Desc", Price = 25.0m, Stock = 80 },
            new Product { Id = 3, Name = "Mid Range", Description = "Desc", Price = 50.0m, Stock = 50 },
            new Product { Id = 4, Name = "Premium", Description = "Desc", Price = 100.0m, Stock = 20 },
            new Product { Id = 5, Name = "Luxury", Description = "Desc", Price = 200.0m, Stock = 10 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(minPrice: 25.0m, maxPrice: 100.0m);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(3, paginatedResponse.Items.Count());
        Assert.All(paginatedResponse.Items, p => Assert.True(p.Price >= 25.0m && p.Price <= 100.0m));
    }

    [Fact]
    public async Task FilterProducts_WithMinStock_ReturnsProductsAboveMinStock()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Low Stock", Description = "Desc", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Medium Stock", Description = "Desc", Price = 20.0m, Stock = 25 },
            new Product { Id = 3, Name = "High Stock", Description = "Desc", Price = 30.0m, Stock = 100 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(minStock: 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.All(paginatedResponse.Items, p => Assert.True(p.Stock >= 20));
    }

    [Fact]
    public async Task FilterProducts_WithMaxStock_ReturnsProductsBelowMaxStock()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Low Stock", Description = "Desc", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Medium Stock", Description = "Desc", Price = 20.0m, Stock = 25 },
            new Product { Id = 3, Name = "High Stock", Description = "Desc", Price = 30.0m, Stock = 100 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(maxStock: 25);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.All(paginatedResponse.Items, p => Assert.True(p.Stock <= 25));
    }

    [Fact]
    public async Task FilterProducts_WithStockRange_ReturnsProductsWithinRange()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Very Low", Description = "Desc", Price = 10.0m, Stock = 2 },
            new Product { Id = 2, Name = "Low", Description = "Desc", Price = 20.0m, Stock = 10 },
            new Product { Id = 3, Name = "Medium", Description = "Desc", Price = 30.0m, Stock = 50 },
            new Product { Id = 4, Name = "High", Description = "Desc", Price = 40.0m, Stock = 100 },
            new Product { Id = 5, Name = "Very High", Description = "Desc", Price = 50.0m, Stock = 500 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(minStock: 10, maxStock: 100);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(3, paginatedResponse.Items.Count());
        Assert.All(paginatedResponse.Items, p => Assert.True(p.Stock >= 10 && p.Stock <= 100));
    }

    [Fact]
    public async Task FilterProducts_WithCombinedFilters_ReturnsMatchingProducts()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Wireless Mouse", Description = "Gaming accessory", Price = 50.0m, Stock = 30 },
            new Product { Id = 2, Name = "Wired Mouse", Description = "Basic mouse", Price = 15.0m, Stock = 100 },
            new Product { Id = 3, Name = "Wireless Keyboard", Description = "Bluetooth keyboard", Price = 80.0m, Stock = 20 },
            new Product { Id = 4, Name = "Gaming Monitor", Description = "High refresh display", Price = 400.0m, Stock = 10 },
            new Product { Id = 5, Name = "Wireless Headset", Description = "Gaming audio", Price = 120.0m, Stock = 5 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Search for "Wireless" products priced between 40 and 130 with stock >= 15
        var result = await controller.FilterProducts(
            searchTerm: "Wireless",
            minPrice: 40.0m,
            maxPrice: 130.0m,
            minStock: 15);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
        Assert.Contains(paginatedResponse.Items, p => p.Name == "Wireless Mouse");
        Assert.Contains(paginatedResponse.Items, p => p.Name == "Wireless Keyboard");
    }

    [Fact]
    public async Task FilterProducts_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        for (int i = 1; i <= 25; i++)
        {
            context.Products.Add(new Product 
            { 
                Id = i, 
                Name = $"Product {i:D2}", 
                Description = "Description", 
                Price = 10.0m * i, 
                Stock = 100 
            });
        }
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Get page 2 with 5 items per page
        var result = await controller.FilterProducts(page: 2, pageSize: 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(5, paginatedResponse.Items.Count());
        Assert.Equal(2, paginatedResponse.Page);
        Assert.Equal(5, paginatedResponse.PageSize);
        Assert.Equal(25, paginatedResponse.TotalCount);
        Assert.Equal(5, paginatedResponse.TotalPages);
        Assert.True(paginatedResponse.HasNextPage);
        Assert.True(paginatedResponse.HasPreviousPage);
    }

    [Fact]
    public async Task FilterProducts_WithPaginationAndFilters_AppliesFiltersBeforePaginating()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        for (int i = 1; i <= 20; i++)
        {
            context.Products.Add(new Product 
            { 
                Id = i, 
                Name = i <= 10 ? $"Alpha {i:D2}" : $"Beta {i:D2}", 
                Description = "Description", 
                Price = 10.0m * i, 
                Stock = 100 
            });
        }
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Filter for "Alpha" products with pagination
        var result = await controller.FilterProducts(searchTerm: "Alpha", page: 1, pageSize: 5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(5, paginatedResponse.Items.Count());
        Assert.Equal(10, paginatedResponse.TotalCount); // Total Alpha products
        Assert.Equal(2, paginatedResponse.TotalPages);
        Assert.All(paginatedResponse.Items, p => Assert.Contains("Alpha", p.Name));
    }

    [Fact]
    public async Task FilterProducts_WithNoMatchingProducts_ReturnsEmptyResult()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Product2", Description = "Desc2", Price = 20.0m, Stock = 10 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Search for non-existent term
        var result = await controller.FilterProducts(searchTerm: "NonExistentProduct");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Empty(paginatedResponse.Items);
        Assert.Equal(0, paginatedResponse.TotalCount);
    }

    [Fact]
    public async Task FilterProducts_WithEmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Empty(paginatedResponse.Items);
        Assert.Equal(0, paginatedResponse.TotalCount);
    }

    [Fact]
    public async Task FilterProducts_WithInvalidPriceRange_ReturnsEmptyResult()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 50.0m, Stock = 5 },
            new Product { Id = 2, Name = "Product2", Description = "Desc2", Price = 75.0m, Stock = 10 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - minPrice > maxPrice (invalid range)
        var result = await controller.FilterProducts(minPrice: 100.0m, maxPrice: 50.0m);

        // Assert - Returns empty because no products match impossible range
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Empty(paginatedResponse.Items);
        Assert.Equal(0, paginatedResponse.TotalCount);
    }

    [Fact]
    public async Task FilterProducts_WithInvalidStockRange_ReturnsEmptyResult()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10.0m, Stock = 20 },
            new Product { Id = 2, Name = "Product2", Description = "Desc2", Price = 20.0m, Stock = 50 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - minStock > maxStock (invalid range)
        var result = await controller.FilterProducts(minStock: 100, maxStock: 10);

        // Assert - Returns empty because no products match impossible range
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Empty(paginatedResponse.Items);
        Assert.Equal(0, paginatedResponse.TotalCount);
    }

    [Fact]
    public async Task FilterProducts_WithNegativePage_ClampsToPage1()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Product2", Description = "Desc2", Price = 20.0m, Stock = 10 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(page: -5);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(1, paginatedResponse.Page);
        Assert.Equal(2, paginatedResponse.Items.Count());
    }

    [Fact]
    public async Task FilterProducts_WithExcessivePageSize_ClampsTo100()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        for (int i = 1; i <= 150; i++)
        {
            context.Products.Add(new Product 
            { 
                Id = i, 
                Name = $"Product {i}", 
                Description = "Description", 
                Price = 10.0m, 
                Stock = 100 
            });
        }
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(pageSize: 500);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(100, paginatedResponse.PageSize);
        Assert.Equal(100, paginatedResponse.Items.Count());
    }

    [Fact]
    public async Task FilterProducts_ResultsAreOrderedByNameThenById()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 3, Name = "Zebra", Description = "Desc", Price = 10.0m, Stock = 5 },
            new Product { Id = 1, Name = "Apple", Description = "Desc", Price = 20.0m, Stock = 10 },
            new Product { Id = 4, Name = "Apple", Description = "Desc2", Price = 15.0m, Stock = 8 },
            new Product { Id = 2, Name = "Banana", Description = "Desc", Price = 30.0m, Stock = 15 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        var products = paginatedResponse.Items.ToList();
        
        Assert.Equal(4, products.Count);
        // First two should be "Apple" ordered by Id
        Assert.Equal("Apple", products[0].Name);
        Assert.Equal(1, products[0].Id);
        Assert.Equal("Apple", products[1].Name);
        Assert.Equal(4, products[1].Id);
        // Then "Banana"
        Assert.Equal("Banana", products[2].Name);
        // Then "Zebra"
        Assert.Equal("Zebra", products[3].Name);
    }

    [Fact]
    public async Task FilterProducts_WithWhitespaceSearchTerm_TreatsAsNoFilter()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Product1", Description = "Desc1", Price = 10.0m, Stock = 5 },
            new Product { Id = 2, Name = "Product2", Description = "Desc2", Price = 20.0m, Stock = 10 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act
        var result = await controller.FilterProducts(searchTerm: "   ");

        // Assert - Should return all products
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(2, paginatedResponse.Items.Count());
    }

    [Fact]
    public async Task FilterProducts_WithExactBoundaryValues_IncludesBoundaryProducts()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.AddRange(
            new Product { Id = 1, Name = "Exact Min", Description = "Desc", Price = 50.0m, Stock = 10 },
            new Product { Id = 2, Name = "Exact Max", Description = "Desc", Price = 100.0m, Stock = 50 },
            new Product { Id = 3, Name = "Middle", Description = "Desc", Price = 75.0m, Stock = 30 }
        );
        await context.SaveChangesAsync();
        
        var logger = Substitute.For<ILogger<ProductsController>>();
        var localizer = GetMockLocalizer();
        var controller = new ProductsController(context, logger, localizer);

        // Act - Use exact boundary values
        var result = await controller.FilterProducts(minPrice: 50.0m, maxPrice: 100.0m, minStock: 10, maxStock: 50);

        // Assert - Should include products at exact boundary values
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginatedResponse = Assert.IsType<PaginatedResponse<Product>>(okResult.Value);
        Assert.Equal(3, paginatedResponse.Items.Count());
    }

    #endregion
}
