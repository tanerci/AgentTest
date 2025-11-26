namespace ProductApi.DTOs;

/// <summary>
/// Data transfer object for updating an existing product.
/// All fields are optional - only include fields you want to update.
/// </summary>
public class ProductUpdateDto
{
    /// <summary>
    /// Gets or sets the updated name of the product (optional).
    /// </summary>
    /// <example>Updated Product Name</example>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the updated description of the product (optional).
    /// </summary>
    /// <example>Updated product description</example>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the updated price of the product (optional).
    /// </summary>
    /// <example>149.99</example>
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Gets or sets the updated stock quantity (optional).
    /// </summary>
    /// <example>25</example>
    public int? Stock { get; set; }
}
