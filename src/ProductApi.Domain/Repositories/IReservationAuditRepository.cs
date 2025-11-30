using ProductApi.Domain.Entities;
using ProductApi.Domain.ValueObjects;

namespace ProductApi.Domain.Repositories;

/// <summary>
/// Repository interface for ReservationAudit persistence.
/// Provides audit trail for all reservation operations.
/// </summary>
public interface IReservationAuditRepository
{
    /// <summary>
    /// Adds a new audit record.
    /// </summary>
    Task<ReservationAuditEntity> AddAsync(ReservationAuditEntity audit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all audit records for a specific reservation.
    /// </summary>
    Task<IEnumerable<ReservationAuditEntity>> GetByReservationIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default);
}
