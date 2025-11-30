using System.ComponentModel.DataAnnotations;

namespace ProductApi.Application.DTOs;

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
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 100 characters")]
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the updated description of the product (optional).
    /// </summary>
    /// <example>Updated product description</example>
    [StringLength(500, ErrorMessage = "Product description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the updated price of the product (optional).
    /// </summary>
    /// <example>149.99</example>
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
    public decimal? Price { get; set; }
    
    /// <summary>
    /// Gets or sets the updated stock quantity (optional).
    /// </summary>
    /// <example>25</example>
    [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number")]
    public int? Stock { get; set; }
}
