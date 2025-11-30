using ProductApi.Application.DTOs;
using ProductApi.Common;

namespace ProductApi.Application.Services;

/// <summary>
/// Interface for product-related application services.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets products with pagination.
    /// </summary>
    Task<Result<PaginatedResponse<ProductDto>>> GetProductsAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters products based on specified criteria.
    /// </summary>
    Task<Result<PaginatedResponse<ProductDto>>> FilterProductsAsync(
        string? searchTerm,
        decimal? minPrice,
        decimal? maxPrice,
        int? minStock,
        int? maxStock,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    Task<Result<ProductDto>> CreateProductAsync(ProductCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Task<Result<ProductDto>> UpdateProductAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    Task<Result> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
