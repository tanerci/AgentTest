using ProductApi.Domain.Entities;

namespace ProductApi.Domain.Services;

/// <summary>
/// Implementation of the reservation domain service.
/// Contains complex business logic for reservation operations.
/// </summary>
public class ReservationDomainService : IReservationDomainService
{
    /// <inheritdoc />
    public bool CanReserve(ProductEntity product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (quantity <= 0)
            return false;

        return product.HasSufficientStock(quantity);
    }

    /// <inheritdoc />
    public ReservationEntity CreateReservation(ProductEntity product, int quantity, int ttlMinutes = 15)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (!CanReserve(product, quantity))
            throw new InvalidOperationException($"Insufficient stock to reserve {quantity} units of product {product.Id}");

        // Create the reservation
        var reservation = ReservationEntity.Create(product.Id, quantity, ttlMinutes);

        // Decrease available stock
        product.RemoveStock(quantity);

        return reservation;
    }

    /// <inheritdoc />
    public void CompleteReservation(ReservationEntity reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        if (!reservation.IsValid)
            throw new InvalidOperationException("Reservation is not valid for checkout");

        reservation.Complete();
    }

    /// <inheritdoc />
    public void CancelReservation(ReservationEntity reservation, ProductEntity product)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(product);

        if (reservation.ProductId != product.Id)
            throw new InvalidOperationException("Reservation product mismatch");

        if (!reservation.Status.CanCancel)
            throw new InvalidOperationException($"Cannot cancel reservation with status: {reservation.Status}");

        // Restore the stock
        product.AddStock(reservation.Quantity);

        // Mark reservation as cancelled
        reservation.Cancel();
    }

    /// <inheritdoc />
    public void ExpireReservation(ReservationEntity reservation, ProductEntity product)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(product);

        if (reservation.ProductId != product.Id)
            throw new InvalidOperationException("Reservation product mismatch");

        if (!reservation.Status.IsActive)
            return; // Already finalized, no-op

        // Restore the stock
        product.AddStock(reservation.Quantity);

        // Mark reservation as expired
        reservation.Expire();
    }
}
