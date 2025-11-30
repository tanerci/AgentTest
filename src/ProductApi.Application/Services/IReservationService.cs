using ProductApi.Application.Common;
using ProductApi.Application.DTOs;

namespace ProductApi.Application.Services;

/// <summary>
/// Interface for reservation-related application services.
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Creates a new reservation for a product.
    /// </summary>
    /// <param name="request">The reservation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the reservation details or an error.</returns>
    Task<Result<ReservationDto>> ReserveAsync(ReserveRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks out a reservation, completing the order.
    /// </summary>
    /// <param name="request">The checkout request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the updated reservation details or an error.</returns>
    Task<Result<ReservationDto>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an active reservation, returning stock to available inventory.
    /// </summary>
    /// <param name="reservationId">The ID of the reservation to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<ReservationDto>> CancelAsync(string reservationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a reservation by its ID.
    /// </summary>
    /// <param name="reservationId">The reservation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the reservation details or an error.</returns>
    Task<Result<ReservationDto>> GetByIdAsync(string reservationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes expired reservations.
    /// Called by a background service or scheduled job.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of reservations expired.</returns>
    Task<int> ProcessExpiredReservationsAsync(CancellationToken cancellationToken = default);
}
