namespace ProductApi.Infrastructure.Persistence.Models;

/// <summary>
/// Represents a reservation in the database (persistence model).
/// </summary>
public class Reservation
{
    /// <summary>
    /// Gets or sets the unique identifier for the reservation.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the reserved product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the reserved quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the current status of the reservation.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the reservation was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the reservation expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets when the reservation was completed or cancelled.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Navigation property to the Product.
    /// </summary>
    public Product? Product { get; set; }
}
