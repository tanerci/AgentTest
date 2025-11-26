using Microsoft.AspNetCore.Mvc;
using ProductApi.Common;
using ProductApi.DTOs;
using ProductApi.Extensions;

namespace ProductApi.Controllers.Examples;

/// <summary>
/// Example controller demonstrating Result pattern usage.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    // Example 1: Simple success/failure with extension method
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var result = await GetProductByIdAsync(id);
        return result.ToActionResult();
    }

    // Example 2: Pattern matching for custom responses
    [HttpGet("advanced/{id}")]
    public async Task<ActionResult<Product>> GetProductAdvanced(int id)
    {
        var result = await GetProductByIdAsync(id);
        
        return result.Match(
            onSuccess: product => Ok(product),
            onFailure: error => error.ToProblemDetails()
        );
    }

    // Example 3: Custom success status code (Created)
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto dto)
    {
        var result = await CreateProductAsync(dto);
        
        return result.ToActionResult(product => 
            CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product)
        );
    }

    // Example 4: Validation with Result pattern
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, ProductUpdateDto dto)
    {
        // Early validation
        if (id <= 0)
            return Result<Product>.ValidationError(
                "Invalid product ID",
                new Dictionary<string, string[]> 
                { 
                    ["id"] = new[] { "Product ID must be greater than 0" } 
                }
            ).ToActionResult();

        var result = await UpdateProductAsync(id, dto);
        return result.ToActionResult();
    }

    // Example service methods that return Result<T>
    private async Task<Result<Product>> GetProductByIdAsync(int id)
    {
        await Task.Delay(10); // Simulate async work

        // Simulate not found scenario
        if (id <= 0)
            return Result<Product>.NotFound($"Product with ID {id} not found");

        // Simulate success
        var product = new Product { Id = id, Name = "Sample Product" };
        return Result<Product>.Success(product);
    }

    private async Task<Result<Product>> CreateProductAsync(ProductCreateDto dto)
    {
        await Task.Delay(10);

        // Validation
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<Product>.ValidationError(
                "Product name is required",
                new Dictionary<string, string[]>
                {
                    ["name"] = new[] { "Name cannot be empty" }
                }
            );

        // Simulate duplicate check
        if (dto.Name == "Duplicate")
            return Result<Product>.Conflict("A product with this name already exists");

        // Success
        var product = new Product { Id = 1, Name = dto.Name };
        return Result<Product>.Success(product);
    }

    private async Task<Result<Product>> UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        await Task.Delay(10);

        // Check if exists
        if (id > 100)
            return Result<Product>.NotFound($"Product {id} not found");

        // Update and return
        var product = new Product { Id = id, Name = dto.Name ?? "Updated" };
        return Result<Product>.Success(product);
    }
}

// Example models
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
