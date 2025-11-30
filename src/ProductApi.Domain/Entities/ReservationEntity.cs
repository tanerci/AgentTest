using ProductApi.Domain.ValueObjects;

namespace ProductApi.Domain.Entities;

/// <summary>
/// Domain entity representing a product reservation.
/// Encapsulates business rules for reservation management.
/// </summary>
public class ReservationEntity
{
    public ReservationId Id { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // Required for EF Core
    private ReservationEntity()
    {
        Id = null!;
        Status = null!;
    }

    private ReservationEntity(
        ReservationId id,
        int productId,
        int quantity,
        ReservationStatus status,
        DateTime createdAt,
        DateTime expiresAt)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        Status = status;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Factory method to create a new reservation.
    /// </summary>
    /// <param name="productId">The ID of the product to reserve.</param>
    /// <param name="quantity">The quantity to reserve.</param>
    /// <param name="ttlMinutes">Time-to-live in minutes for the reservation.</param>
    /// <returns>A new ReservationEntity.</returns>
    public static ReservationEntity Create(int productId, int quantity, int ttlMinutes = 15)
    {
        if (productId <= 0)
            throw new ArgumentException("Product ID must be positive", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (ttlMinutes <= 0)
            throw new ArgumentException("TTL must be positive", nameof(ttlMinutes));

        var now = DateTime.UtcNow;
        return new ReservationEntity(
            ReservationId.Create(),
            productId,
            quantity,
            ReservationStatus.Reserved,
            now,
            now.AddMinutes(ttlMinutes));
    }

    /// <summary>
    /// Factory method to hydrate a reservation from persistence.
    /// </summary>
    internal static ReservationEntity Hydrate(
        Guid id,
        int productId,
        int quantity,
        string status,
        DateTime createdAt,
        DateTime expiresAt,
        DateTime? completedAt)
    {
        var entity = new ReservationEntity(
            ReservationId.FromGuid(id),
            productId,
            quantity,
            ReservationStatus.FromString(status),
            createdAt,
            expiresAt)
        {
            CompletedAt = completedAt
        };
        return entity;
    }

    /// <summary>
    /// Marks the reservation as completed (checked out).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when reservation cannot be checked out.</exception>
    public void Complete()
    {
        if (!Status.CanCheckout)
            throw new InvalidOperationException($"Cannot checkout reservation with status: {Status}");

        Status = ReservationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the reservation as expired.
    /// </summary>
    public void Expire()
    {
        if (!Status.IsActive)
            return; // Already finalized, no-op

        Status = ReservationStatus.Expired;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the reservation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when reservation cannot be cancelled.</exception>
    public void Cancel()
    {
        if (!Status.CanCancel)
            throw new InvalidOperationException($"Cannot cancel reservation with status: {Status}");

        Status = ReservationStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the reservation has expired based on current time.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if the reservation is still valid and can be used.
    /// </summary>
    public bool IsValid => Status.IsActive && !IsExpired;
}
