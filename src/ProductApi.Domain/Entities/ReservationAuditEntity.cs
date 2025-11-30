using ProductApi.Domain.ValueObjects;

namespace ProductApi.Domain.Entities;

/// <summary>
/// Domain entity representing an audit record for reservation operations.
/// Provides a complete history of reservation state changes.
/// </summary>
public class ReservationAuditEntity
{
    public int Id { get; private set; }
    public ReservationId ReservationId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? Notes { get; private set; }

    // Required for EF Core
    private ReservationAuditEntity()
    {
        ReservationId = null!;
        Status = null!;
    }

    private ReservationAuditEntity(
        ReservationId reservationId,
        int productId,
        int quantity,
        ReservationStatus status,
        DateTime timestamp,
        string? notes)
    {
        ReservationId = reservationId;
        ProductId = productId;
        Quantity = quantity;
        Status = status;
        Timestamp = timestamp;
        Notes = notes;
    }

    /// <summary>
    /// Factory method to create an audit record from a reservation entity.
    /// </summary>
    public static ReservationAuditEntity Create(ReservationEntity reservation, string? notes = null)
    {
        return new ReservationAuditEntity(
            reservation.Id,
            reservation.ProductId,
            reservation.Quantity,
            reservation.Status,
            DateTime.UtcNow,
            notes);
    }

    /// <summary>
    /// Factory method to hydrate an audit record from persistence.
    /// </summary>
    internal static ReservationAuditEntity Hydrate(
        int id,
        Guid reservationId,
        int productId,
        int quantity,
        string status,
        DateTime timestamp,
        string? notes)
    {
        return new ReservationAuditEntity(
            ReservationId.FromGuid(reservationId),
            productId,
            quantity,
            ReservationStatus.FromString(status),
            timestamp,
            notes)
        {
            Id = id
        };
    }
}
