namespace ProductApi.DTOs;

/// <summary>
/// Data transfer object for creating a new product.
/// </summary>
public class ProductCreateDto
{
    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    /// <example>Wireless Mouse</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    /// <example>Ergonomic wireless mouse with USB receiver</example>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    /// <example>29.99</example>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the initial stock quantity.
    /// </summary>
    /// <example>50</example>
    public int Stock { get; set; }
}
