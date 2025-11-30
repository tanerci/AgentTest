namespace ProductApi.Infrastructure.Persistence.Models;

/// <summary>
/// Represents an audit record for reservation operations (persistence model).
/// </summary>
public class ReservationAudit
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the reservation this audit belongs to.
    /// </summary>
    public Guid ReservationId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the product at the time of the operation.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity at the time of the operation.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the status at the time of the operation.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the audit record was created.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets optional notes for the operation.
    /// </summary>
    public string? Notes { get; set; }
}
