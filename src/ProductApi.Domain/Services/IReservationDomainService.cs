using ProductApi.Domain.Entities;

namespace ProductApi.Domain.Services;

/// <summary>
/// Domain service for orchestrating reservation business logic.
/// Contains complex business rules that span multiple entities.
/// </summary>
public interface IReservationDomainService
{
    /// <summary>
    /// Validates if a reservation can be created for the specified product and quantity.
    /// </summary>
    /// <param name="product">The product entity to reserve.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <returns>True if the reservation can be created; false otherwise.</returns>
    bool CanReserve(ProductEntity product, int quantity);

    /// <summary>
    /// Creates a reservation entity after validating business rules.
    /// </summary>
    /// <param name="product">The product entity to reserve.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="ttlMinutes">Time-to-live for the reservation in minutes.</param>
    /// <returns>The created reservation entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown when insufficient stock is available.</exception>
    ReservationEntity CreateReservation(ProductEntity product, int quantity, int ttlMinutes = 15);

    /// <summary>
    /// Completes a reservation, effectively removing the reserved stock.
    /// </summary>
    /// <param name="reservation">The reservation to complete.</param>
    /// <exception cref="InvalidOperationException">Thrown when reservation cannot be completed.</exception>
    void CompleteReservation(ReservationEntity reservation);

    /// <summary>
    /// Cancels a reservation, returning the stock to available inventory.
    /// </summary>
    /// <param name="reservation">The reservation to cancel.</param>
    /// <param name="product">The product entity to restore stock to.</param>
    /// <exception cref="InvalidOperationException">Thrown when reservation cannot be cancelled.</exception>
    void CancelReservation(ReservationEntity reservation, ProductEntity product);

    /// <summary>
    /// Expires a reservation, returning the stock to available inventory.
    /// </summary>
    /// <param name="reservation">The reservation to expire.</param>
    /// <param name="product">The product entity to restore stock to.</param>
    void ExpireReservation(ReservationEntity reservation, ProductEntity product);
}
