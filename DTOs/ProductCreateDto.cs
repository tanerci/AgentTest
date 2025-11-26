using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    /// <example>Ergonomic wireless mouse with USB receiver</example>
    [Required(ErrorMessage = "Product description is required")]
    [StringLength(500, ErrorMessage = "Product description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    /// <example>29.99</example>
    [Required(ErrorMessage = "Product price is required")]
    [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999999.99")]
    public decimal Price { get; set; }
    
    /// <summary>
    /// Gets or sets the initial stock quantity.
    /// </summary>
    /// <example>50</example>
    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number")]
    public int Stock { get; set; }
}
