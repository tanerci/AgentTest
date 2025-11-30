using System.ComponentModel.DataAnnotations;

namespace ProductApi.Application.DTOs;

/// <summary>
/// Request DTO for creating a product reservation.
/// </summary>
public class ReserveRequest
{
    /// <summary>
    /// The ID of the product to reserve.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer")]
    public int ProductId { get; set; }

    /// <summary>
    /// The quantity to reserve.
    /// </summary>
    [Required]
    [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
    public int Quantity { get; set; }

    /// <summary>
    /// Optional TTL in minutes for the reservation. Defaults to 15 minutes.
    /// </summary>
    [Range(1, 60, ErrorMessage = "TTL must be between 1 and 60 minutes")]
    public int TtlMinutes { get; set; } = 15;
}
