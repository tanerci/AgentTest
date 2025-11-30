using System.ComponentModel.DataAnnotations;

namespace ProductApi.Application.DTOs;

/// <summary>
/// Request DTO for checking out a reservation.
/// </summary>
public class CheckoutRequest
{
    /// <summary>
    /// The ID of the reservation to checkout.
    /// </summary>
    [Required]
    public string ReservationId { get; set; } = string.Empty;
}
