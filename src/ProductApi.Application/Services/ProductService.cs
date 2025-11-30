using Microsoft.Extensions.Logging;
using ProductApi.Application.Common;
using ProductApi.Application.DTOs;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;

namespace ProductApi.Application.Services;

/// <summary>
/// Application service for product-related business operations.
/// Orchestrates domain objects and repositories to fulfill use cases.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResponse<ProductDto>>> GetProductsAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);

        _logger.LogInformation("Retrieving products (page: {Page}, pageSize: {PageSize})", validPage, validPageSize);

        var (products, totalCount) = await _productRepository.GetAllAsync(validPage, validPageSize, cancellationToken);
        
        var productDtos = products.Select(MapToDto).ToList();

        var response = new PaginatedResponse<ProductDto>
        {
            Items = productDtos,
            Page = validPage,
            PageSize = validPageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation("Retrieved {Count} products (page {Page} of {TotalPages})", 
            productDtos.Count, validPage, response.TotalPages);

        return Result.Success(response);
    }

    public async Task<Result<PaginatedResponse<ProductDto>>> FilterProductsAsync(
        string? searchTerm,
        decimal? minPrice,
        decimal? maxPrice,
        int? minStock,
        int? maxStock,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Validate range parameters
        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
        {
            return Result.Failure<PaginatedResponse<ProductDto>>(
                Error.Validation($"minPrice ({minPrice}) cannot be greater than maxPrice ({maxPrice})"));
        }
        
        if (minStock.HasValue && maxStock.HasValue && minStock.Value > maxStock.Value)
        {
            return Result.Failure<PaginatedResponse<ProductDto>>(
                Error.Validation($"minStock ({minStock}) cannot be greater than maxStock ({maxStock})"));
        }

        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);

        _logger.LogInformation(
            "Filtering products - SearchTerm: {SearchTerm}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, MinStock: {MinStock}, MaxStock: {MaxStock}, Page: {Page}, PageSize: {PageSize}",
            searchTerm ?? "none", minPrice?.ToString() ?? "none", maxPrice?.ToString() ?? "none", 
            minStock?.ToString() ?? "none", maxStock?.ToString() ?? "none", validPage, validPageSize);

        var (products, totalCount) = await _productRepository.FilterAsync(
            searchTerm, minPrice, maxPrice, minStock, maxStock, validPage, validPageSize, cancellationToken);

        var productDtos = products.Select(MapToDto).ToList();

        var response = new PaginatedResponse<ProductDto>
        {
            Items = productDtos,
            Page = validPage,
            PageSize = validPageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "Retrieved {Count} filtered products (page {Page} of {TotalPages}, total matching: {TotalCount})", 
            productDtos.Count, validPage, response.TotalPages, totalCount);

        return Result.Success(response);
    }

    public async Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", id);
            return Result.Failure<ProductDto>(Error.NotFound($"Product with ID {id} not found"));
        }

        _logger.LogInformation("Retrieved product: {ProductName} (ID: {ProductId})", product.Name.Value, product.Id);
        return Result.Success(MapToDto(product));
    }

    public async Task<Result<ProductDto>> CreateProductAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new product: {ProductName}", dto.Name);

        try
        {
            var product = ProductEntity.Create(dto.Name, dto.Description, dto.Price, dto.Stock);
            var createdProduct = await _productRepository.AddAsync(product, cancellationToken);

            _logger.LogInformation("Created product: {ProductName} (ID: {ProductId})", 
                createdProduct.Name.Value, createdProduct.Id);

            return Result.Success(MapToDto(createdProduct));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to create product due to validation error: {Message}", ex.Message);
            return Result.Failure<ProductDto>(Error.Validation(ex.Message));
        }
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Update failed: Product with ID {ProductId} not found", id);
            return Result.Failure<ProductDto>(Error.NotFound($"Product with ID {id} not found"));
        }

        try
        {
            if (dto.Name != null)
                product.UpdateName(dto.Name);

            if (dto.Description != null)
                product.UpdateDescription(dto.Description);

            if (dto.Price.HasValue)
                product.UpdatePrice(dto.Price.Value);

            if (dto.Stock.HasValue)
                product.UpdateStock(dto.Stock.Value);

            await _productRepository.UpdateAsync(product, cancellationToken);

            _logger.LogInformation("Updated product: {ProductName} (ID: {ProductId})", product.Name.Value, product.Id);
            return Result.Success(MapToDto(product));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to update product due to validation error: {Message}", ex.Message);
            return Result.Failure<ProductDto>(Error.Validation(ex.Message));
        }
    }

    public async Task<Result> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Delete failed: Product with ID {ProductId} not found", id);
            return Result.Failure(Error.NotFound($"Product with ID {id} not found"));
        }

        var productName = product.Name.Value;
        await _productRepository.DeleteAsync(product, cancellationToken);

        _logger.LogInformation("Deleted product: {ProductName} (ID: {ProductId})", productName, id);
        return Result.Success();
    }

    private static ProductDto MapToDto(ProductEntity entity)
    {
        return new ProductDto
        {
            Id = entity.Id,
            Name = entity.Name.Value,
            Description = entity.Description,
            Price = entity.Price.Amount,
            Stock = entity.Stock.Quantity
        };
    }
}
