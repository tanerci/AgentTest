using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;

namespace ProductApi.Controllers;

/// <summary>
/// Manages product inventory including CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the ProductsController.
    /// </summary>
    /// <param name="context">The database context for product operations.</param>
    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all products from the inventory.
    /// </summary>
    /// <returns>A list of all products.</returns>
    /// <response code="200">Returns the list of all products.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/products
    ///     
    /// This endpoint is publicly accessible and does not require authentication.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    /// <summary>
    /// Retrieves a specific product by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>The product with the specified ID.</returns>
    /// <response code="200">Returns the requested product.</response>
    /// <response code="404">Product with the specified ID not found.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/products/1
    ///     
    /// This endpoint is publicly accessible and does not require authentication.
    /// </remarks>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return Ok(product);
    }

    /// <summary>
    /// Creates a new product in the inventory.
    /// </summary>
    /// <param name="productDto">The product information to create.</param>
    /// <returns>The newly created product.</returns>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">Invalid request - validation errors.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/products
    ///     {
    ///        "name": "Wireless Mouse",
    ///        "description": "Ergonomic wireless mouse with USB receiver",
    ///        "price": 29.99,
    ///        "stock": 50
    ///     }
    ///     
    /// This endpoint requires authentication. Include the authentication cookie in the request.
    /// </remarks>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] ProductCreateDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Stock = productDto.Stock
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Updates an existing product's information.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="productDto">The product fields to update (all fields are optional).</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">Product updated successfully.</response>
    /// <response code="400">Invalid request - validation errors.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Product with the specified ID not found.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     PUT /api/products/1
    ///     {
    ///        "name": "Updated Product Name",
    ///        "price": 149.99
    ///     }
    ///     
    /// All fields are optional. Only include the fields you want to update.
    /// This endpoint requires authentication. Include the authentication cookie in the request.
    /// </remarks>
    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto productDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        if (productDto.Name != null)
            product.Name = productDto.Name;
        
        if (productDto.Description != null)
            product.Description = productDto.Description;
        
        if (productDto.Price.HasValue)
            product.Price = productDto.Price.Value;
        
        if (productDto.Stock.HasValue)
            product.Stock = productDto.Stock.Value;

        await _context.SaveChangesAsync();

        return Ok(product);
    }

    /// <summary>
    /// Deletes a product from the inventory.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <returns>Success message confirming deletion.</returns>
    /// <response code="200">Product deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Product with the specified ID not found.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     DELETE /api/products/1
    ///     
    /// This endpoint requires authentication. Include the authentication cookie in the request.
    /// This operation is permanent and cannot be undone.
    /// </remarks>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Product deleted successfully" });
    }
}
