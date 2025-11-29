using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ProductApi.Application.Services;
using ProductApi.DTOs;
using ProductApi.Extensions;
using ProductApi.Models;
using ProductApi.Resources;

namespace ProductApi.Controllers;

/// <summary>
/// Manages product inventory including CRUD operations.
/// Thin controller that delegates to the ProductService following DDD principles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    /// <summary>
    /// Initializes a new instance of the ProductsController.
    /// </summary>
    /// <param name="productService">The product application service.</param>
    /// <param name="localizer">The string localizer for localized messages.</param>
    public ProductsController(IProductService productService, IStringLocalizer<SharedResource> localizer)
    {
        _productService = productService;
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
        var result = await _productService.GetProductsAsync(page, pageSize);
        
        return result.Match(
            response => Ok(MapToPaginatedProductResponse(response)),
            error => error.ToProblemDetails());
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
        var result = await _productService.FilterProductsAsync(
            searchTerm, minPrice, maxPrice, minStock, maxStock, page, pageSize);

        return result.Match(
            response => Ok(MapToPaginatedProductResponse(response)),
            error => error.ToProblemDetails());
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
        var result = await _productService.GetProductByIdAsync(id);

        return result.Match(
            product => Ok(MapToProduct(product)),
            error => NotFound(new { message = string.Format(_localizer["ProductNotFound"], id) }));
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

        var result = await _productService.CreateProductAsync(productDto);

        return result.Match(
            product => CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToProduct(product)),
            error => error.ToProblemDetails());
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

        var result = await _productService.UpdateProductAsync(id, productDto);

        return result.Match(
            product => Ok(MapToProduct(product)),
            error => error.Type == Common.ErrorType.NotFound
                ? NotFound(new { message = string.Format(_localizer["ProductNotFound"], id) })
                : error.ToProblemDetails());
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
        var result = await _productService.DeleteProductAsync(id);

        return result.Match(
            () => Ok(new { message = _localizer["ProductDeletedSuccessfully"].Value }),
            error => error.Type == Common.ErrorType.NotFound
                ? NotFound(new { message = string.Format(_localizer["ProductNotFound"], id) })
                : error.ToProblemDetails());
    }

    /// <summary>
    /// Maps ProductDto to Product model for API backward compatibility.
    /// </summary>
    private static Product MapToProduct(ProductDto dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock
        };
    }

    /// <summary>
    /// Maps PaginatedResponse of ProductDto to PaginatedResponse of Product for API backward compatibility.
    /// </summary>
    private static PaginatedResponse<Product> MapToPaginatedProductResponse(PaginatedResponse<ProductDto> response)
    {
        return new PaginatedResponse<Product>
        {
            Items = response.Items.Select(MapToProduct),
            Page = response.Page,
            PageSize = response.PageSize,
            TotalCount = response.TotalCount
        };
    }
}
