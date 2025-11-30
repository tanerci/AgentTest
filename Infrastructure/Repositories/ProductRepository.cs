using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;
using ProductApi.Infrastructure.Persistence;
using ProductApi.Infrastructure.Persistence.Models;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of IProductRepository.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProductEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return model == null ? null : MapToDomainEntity(model);
    }

    public async Task<(IEnumerable<ProductEntity> Items, int TotalCount)> GetAllAsync(
        int page = 1, 
        int pageSize = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsNoTracking();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var products = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products.Select(MapToDomainEntity), totalCount);
    }

    public async Task<(IEnumerable<ProductEntity> Items, int TotalCount)> FilterAsync(
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? minStock = null,
        int? maxStock = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerSearchTerm) || 
                p.Description.ToLower().Contains(lowerSearchTerm));
        }

        // Apply price range filters
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        // Apply stock range filters
        if (minStock.HasValue)
            query = query.Where(p => p.Stock >= minStock.Value);

        if (maxStock.HasValue)
            query = query.Where(p => p.Stock <= maxStock.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (products.Select(MapToDomainEntity), totalCount);
    }

    public async Task<ProductEntity> AddAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        var model = MapToDataModel(product);
        
        _context.Products.Add(model);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDomainEntity(model);
    }

    public async Task UpdateAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        var model = await _context.Products.FindAsync(new object[] { product.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {product.Id} not found");

        model.Name = product.Name.Value;
        model.Description = product.Description;
        model.Price = product.Price.Amount;
        model.Stock = product.Stock.Quantity;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        var model = await _context.Products.FindAsync(new object[] { product.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {product.Id} not found");

        _context.Products.Remove(model);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);
    }

    private static ProductEntity MapToDomainEntity(Product model)
    {
        // Use the Hydrate factory method to safely reconstruct the domain entity from persistence
        return ProductEntity.Hydrate(
            model.Id,
            model.Name,
            model.Description,
            model.Price,
            model.Stock);
    }

    private static Product MapToDataModel(ProductEntity entity)
    {
        return new Product
        {
            Id = entity.Id,
            Name = entity.Name.Value,
            Description = entity.Description,
            Price = entity.Price.Amount,
            Stock = entity.Stock.Quantity
        };
    }
}
