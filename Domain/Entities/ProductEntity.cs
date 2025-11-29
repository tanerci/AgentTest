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

    private ProductEntity(ProductName name, string description, Money price, Stock stock)
    {
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
            ProductName.Create(name),
            description,
            Money.Create(price),
            Stock.Create(stock));
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
