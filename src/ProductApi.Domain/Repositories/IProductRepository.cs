using ProductApi.Domain.Entities;

namespace ProductApi.Domain.Repositories;

/// <summary>
/// Repository interface for Product aggregate root.
/// Defines the contract for product persistence operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products with optional pagination.
    /// </summary>
    Task<(IEnumerable<ProductEntity> Items, int TotalCount)> GetAllAsync(
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches and filters products based on specified criteria.
    /// </summary>
    Task<(IEnumerable<ProductEntity> Items, int TotalCount)> FilterAsync(
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minStock = null,
        int? maxStock = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new product to the repository.
    /// </summary>
    Task<ProductEntity> AddAsync(ProductEntity product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product in the repository.
    /// </summary>
    Task UpdateAsync(ProductEntity product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a product from the repository.
    /// </summary>
    Task DeleteAsync(ProductEntity product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product with the given ID exists.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
