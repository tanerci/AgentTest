namespace ProductApi.Application.DTOs;

/// <summary>
/// DTO for product data returned to clients.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// The unique identifier of the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// The stock quantity.
    /// </summary>
    public int Stock { get; set; }
}
