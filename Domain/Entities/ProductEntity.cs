using ProductApi.Domain.ValueObjects;

namespace ProductApi.Domain.Entities;

/// <summary>
/// Domain entity representing a product in the inventory.
/// Encapsulates business rules for product management.
/// </summary>
public class ProductEntity
{
    public int Id { get; private set; }
    public ProductName Name { get; private set; }
    public string Description { get; private set; }
    public Money Price { get; private set; }
    public Stock Stock { get; private set; }

    // Required for EF Core
    private ProductEntity() 
    { 
        Name = null!;
        Description = string.Empty;
        Price = null!;
        Stock = null!;
    }

    private ProductEntity(int id, ProductName name, string description, Money price, Stock stock)
    {
        Id = id;
        Name = name;
        Description = description ?? string.Empty;
        Price = price;
        Stock = stock;
    }

    /// <summary>
    /// Factory method to create a new Product entity.
    /// </summary>
    public static ProductEntity Create(string name, string description, decimal price, int stock)
    {
        return new ProductEntity(
            0, // ID will be assigned by the database
            ProductName.Create(name),
            description,
            Money.Create(price),
            Stock.Create(stock));
    }

    /// <summary>
    /// Factory method to hydrate a Product entity from persistence.
    /// This method is used by the repository layer to reconstruct domain entities.
    /// </summary>
    /// <remarks>
    /// Uses TryCreate methods for safer value object instantiation from persisted data.
    /// Falls back to default values if persistence data is invalid.
    /// </remarks>
    internal static ProductEntity Hydrate(int id, string name, string description, decimal price, int stock)
    {
        // Use TryCreate for safer hydration from persistence - data should be valid but we handle edge cases
        var productName = ProductName.TryCreate(name) ?? ProductName.Create("Unknown Product");
        var money = Money.TryCreate(price) ?? Money.Create(0);
        var stockValue = Stock.TryCreate(stock) ?? Stock.Create(0);

        return new ProductEntity(id, productName, description ?? string.Empty, money, stockValue);
    }

    /// <summary>
    /// Updates the product name.
    /// </summary>
    public void UpdateName(string name)
    {
        Name = ProductName.Create(name);
    }

    /// <summary>
    /// Updates the product description.
    /// </summary>
    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// Updates the product price.
    /// </summary>
    public void UpdatePrice(decimal price)
    {
        Price = Money.Create(price);
    }

    /// <summary>
    /// Updates the stock quantity.
    /// </summary>
    public void UpdateStock(int stock)
    {
        Stock = Stock.Create(stock);
    }

    /// <summary>
    /// Adds stock to the product.
    /// </summary>
    public void AddStock(int quantity)
    {
        Stock = Stock.Add(quantity);
    }

    /// <summary>
    /// Removes stock from the product.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when there's insufficient stock.</exception>
    public void RemoveStock(int quantity)
    {
        Stock = Stock.Remove(quantity);
    }

    /// <summary>
    /// Checks if the product has sufficient stock for the given quantity.
    /// </summary>
    public bool HasSufficientStock(int quantity) => Stock.HasSufficientStock(quantity);
}
