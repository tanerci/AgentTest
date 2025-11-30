namespace ProductApi.Application.DTOs;

/// <summary>
/// Response DTO for a successful reservation.
/// </summary>
public class ReservationDto
{
    /// <summary>
    /// The unique identifier for the reservation.
    /// </summary>
    public string ReservationId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the reserved product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// The reserved quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// The current status of the reservation.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// When the reservation was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the reservation will expire.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the reservation was completed or cancelled.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
