using ProductApi.Domain.ValueObjects;

namespace ProductApi.Domain.Repositories;

/// <summary>
/// Result of a Redis reservation operation.
/// </summary>
public enum ReservationOperationResult
{
    /// <summary>
    /// Operation succeeded - stock was reserved.
    /// </summary>
    Reserved,

    /// <summary>
    /// Operation failed - insufficient stock available.
    /// </summary>
    InsufficientStock,

    /// <summary>
    /// Checkout succeeded.
    /// </summary>
    CheckedOut,

    /// <summary>
    /// Reservation was not found in cache.
    /// </summary>
    ReservationNotFound,

    /// <summary>
    /// Reservation was expired.
    /// </summary>
    Expired,

    /// <summary>
    /// Reservation was cancelled.
    /// </summary>
    Cancelled
}

/// <summary>
/// Repository interface for Redis-based reservation caching.
/// Handles atomic stock reservation operations with TTL support.
/// </summary>
public interface IRedisReservationRepository
{
    /// <summary>
    /// Atomically reserves stock for a product.
    /// Decrements available stock, increments reserved stock, and sets TTL key.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="reservationId">The unique reservation identifier.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="ttlSeconds">Time-to-live in seconds for the reservation.</param>
    /// <returns>The result of the operation.</returns>
    Task<ReservationOperationResult> ReserveAsync(
        int productId,
        ReservationId reservationId,
        int quantity,
        int ttlSeconds);

    /// <summary>
    /// Atomically checks out a reservation.
    /// Decrements reserved stock and removes the TTL key.
    /// </summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <returns>The result of the operation.</returns>
    Task<ReservationOperationResult> CheckoutAsync(ReservationId reservationId);

    /// <summary>
    /// Atomically cancels/expires a reservation.
    /// Restores available stock, decrements reserved stock, and removes the TTL key.
    /// </summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <returns>The result of the operation.</returns>
    Task<ReservationOperationResult> ReleaseAsync(ReservationId reservationId);

    /// <summary>
    /// Gets the current available stock for a product from cache.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <returns>The available stock, or null if not cached.</returns>
    Task<int?> GetAvailableStockAsync(int productId);

    /// <summary>
    /// Initializes the product stock in cache from the database value.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="availableStock">The available stock quantity.</param>
    Task InitializeStockAsync(int productId, int availableStock);

    /// <summary>
    /// Gets reservation data from cache if it exists.
    /// </summary>
    /// <param name="reservationId">The reservation identifier.</param>
    /// <returns>Reservation data tuple (productId, quantity) or null if not found.</returns>
    Task<(int ProductId, int Quantity)?> GetReservationAsync(ReservationId reservationId);
}
