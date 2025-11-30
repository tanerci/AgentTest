using ProductApi.Domain.Entities;
using ProductApi.Domain.ValueObjects;

namespace ProductApi.Domain.Repositories;

/// <summary>
/// Repository interface for Reservation aggregate root.
/// Defines the contract for reservation persistence operations in SQL Server.
/// </summary>
public interface IReservationRepository
{
    /// <summary>
    /// Gets a reservation by its unique identifier.
    /// </summary>
    Task<ReservationEntity?> GetByIdAsync(ReservationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active reservations for a specific product.
    /// </summary>
    Task<IEnumerable<ReservationEntity>> GetActiveByProductIdAsync(int productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all expired reservations that need to be processed.
    /// </summary>
    Task<IEnumerable<ReservationEntity>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new reservation to the repository.
    /// </summary>
    Task<ReservationEntity> AddAsync(ReservationEntity reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing reservation in the repository.
    /// </summary>
    Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a reservation exists.
    /// </summary>
    Task<bool> ExistsAsync(ReservationId id, CancellationToken cancellationToken = default);
}
