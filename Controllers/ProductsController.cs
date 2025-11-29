using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using ProductApi.Data;
using ProductApi.DTOs;
using ProductApi.Models;
using ProductApi.Resources;

namespace ProductApi.Controllers;

/// <summary>
/// Manages product inventory including CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the ProductsController.
    /// </summary>
    /// <param name="context">The database context for product operations.</param>
    /// <param name="logger">The logger for tracking product operations.</param>
    /// <param name="localizer">The string localizer for localized messages.</param>
    public ProductsController(AppDbContext context, ILogger<ProductsController> logger, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _logger = logger;
        _localizer = localizer;
    }

    /// <summary>
    /// Retrieves products from the inventory with optional pagination.
    /// </summary>
    /// <param name="page">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of items per page (1-100). Default is 10.</param>
    /// <returns>A paginated response containing products and pagination metadata.</returns>
    /// <response code="200">Returns the paginated list of products.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/products
    ///     GET /api/products?page=1&amp;pageSize=10
    ///     
    /// This endpoint is publicly accessible and does not require authentication.
    /// Returns a paginated response with items, page, pageSize, totalCount, and totalPages.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<Product>), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<ActionResult<PaginatedResponse<Product>>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Retrieving products (page: {Page}, pageSize: {PageSize})", page, pageSize);
        
        // Validate and clamp pagination parameters
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        
        // Use AsNoTracking for read-only queries to improve performance
        var query = _context.Products.AsNoTracking();
        
        var totalCount = await query.CountAsync();
        var products = await query
            .OrderBy(p => p.Id)
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync();
        
        var response = new PaginatedResponse<Product>
        {
            Items = products,
            Page = validPage,
            PageSize = validPageSize,
            TotalCount = totalCount
        };
        
        _logger.LogInformation("Retrieved {Count} products (page {Page} of {TotalPages})", 
            products.Count, validPage, response.TotalPages);
        return Ok(response);
    }

    /// <summary>
    /// Searches and filters products based on specified criteria with pagination.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by product name or description.</param>
    /// <param name="minPrice">Optional minimum price filter.</param>
    /// <param name="maxPrice">Optional maximum price filter.</param>
    /// <param name="minStock">Optional minimum stock quantity filter.</param>
    /// <param name="maxStock">Optional maximum stock quantity filter.</param>
    /// <param name="page">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of items per page (1-100). Default is 10.</param>
    /// <returns>A paginated response containing filtered products and pagination metadata.</returns>
    /// <response code="200">Returns the paginated list of filtered products.</response>
    /// <remarks>
    /// Sample requests:
    /// 
    ///     GET /api/products/filter?searchTerm=mouse
    ///     GET /api/products/filter?minPrice=50&amp;maxPrice=200
    ///     GET /api/products/filter?minStock=10
    ///     GET /api/products/filter?searchTerm=laptop&amp;minPrice=500&amp;page=1&amp;pageSize=20
    ///     
    /// This endpoint is publicly accessible and does not require authentication.
    /// All filter parameters are optional and can be combined.
    /// Search term performs case-insensitive partial matching on name and description.
    /// </remarks>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(PaginatedResponse<Product>), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 30, VaryByQueryKeys = ["searchTerm", "minPrice", "maxPrice", "minStock", "maxStock", "page", "pageSize"])]
    public async Task<ActionResult<PaginatedResponse<Product>>> FilterProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? minStock = null,
        [FromQuery] int? maxStock = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(
            "Filtering products - SearchTerm: {SearchTerm}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, MinStock: {MinStock}, MaxStock: {MaxStock}, Page: {Page}, PageSize: {PageSize}",
            searchTerm ?? "none", minPrice?.ToString() ?? "none", maxPrice?.ToString() ?? "none", 
            minStock?.ToString() ?? "none", maxStock?.ToString() ?? "none", page, pageSize);
        
        // Validate range parameters
        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
        {
            return BadRequest("minPrice cannot be greater than maxPrice");
        }
        if (minStock.HasValue && maxStock.HasValue && minStock.Value > maxStock.Value)
        {
            return BadRequest("minStock cannot be greater than maxStock");
        }
        
        // Validate and clamp pagination parameters
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        
        // Build the query with filters
        IQueryable<Product> query = _context.Products.AsNoTracking();
        
        // Apply search term filter (case-insensitive partial match on name and description)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerSearchTerm) || 
                p.Description.ToLower().Contains(lowerSearchTerm));
        }
        
        // Apply price range filters
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }
        
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }
        
        // Apply stock range filters
        if (minStock.HasValue)
        {
            query = query.Where(p => p.Stock >= minStock.Value);
        }
        
        if (maxStock.HasValue)
        {
            query = query.Where(p => p.Stock <= maxStock.Value);
        }
        
        // Get total count after filters
        var totalCount = await query.CountAsync();
        
        // Apply pagination and ordering
        var products = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((validPage - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync();
        
        var response = new PaginatedResponse<Product>
        {
            Items = products,
            Page = validPage,
            PageSize = validPageSize,
            TotalCount = totalCount
        };
        
        _logger.LogInformation(
            "Retrieved {Count} filtered products (page {Page} of {TotalPages}, total matching: {TotalCount})", 
            products.Count, validPage, response.TotalPages, totalCount);
        
        return Ok(response);
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
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);
        
        // Use AsNoTracking for read-only queries to improve performance
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", id);
            return NotFound(new { message = string.Format(_localizer["ProductNotFound"], id) });
        }

        _logger.LogInformation("Retrieved product: {ProductName} (ID: {ProductId})", product.Name, product.Id);
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
            _logger.LogWarning("Create product failed: invalid model state");
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating new product: {ProductName}", productDto.Name);
        var product = new Product
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Price = productDto.Price,
            Stock = productDto.Stock
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created product: {ProductName} (ID: {ProductId})", product.Name, product.Id);
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
            _logger.LogWarning("Update product failed for ID {ProductId}: invalid model state", id);
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Updating product with ID: {ProductId}", id);
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Update failed: Product with ID {ProductId} not found", id);
            return NotFound(new { message = string.Format(_localizer["ProductNotFound"], id) });
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

        _logger.LogInformation("Updated product: {ProductName} (ID: {ProductId})", product.Name, product.Id);
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
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Delete failed: Product with ID {ProductId} not found", id);
            return NotFound(new { message = string.Format(_localizer["ProductNotFound"], id) });
        }

        var productName = product.Name;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted product: {ProductName} (ID: {ProductId})", productName, id);
        return Ok(new { message = _localizer["ProductDeletedSuccessfully"].Value });
    }
}
