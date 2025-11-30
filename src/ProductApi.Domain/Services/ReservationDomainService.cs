using Microsoft.Extensions.Logging;
using ProductApi.Domain.Entities;

namespace ProductApi.Domain.Services;

/// <summary>
/// Implementation of the reservation domain service.
/// Contains complex business logic for reservation operations.
/// </summary>
public class ReservationDomainService : IReservationDomainService
{
    private readonly ILogger<ReservationDomainService> _logger;

    /// <summary>
    /// Initializes a new instance of the ReservationDomainService.
    /// </summary>
    /// <param name="logger">The logger for domain service operations.</param>
    public ReservationDomainService(ILogger<ReservationDomainService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool CanReserve(ProductEntity product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (quantity <= 0)
        {
            _logger.LogDebug("CanReserve check failed: Quantity {Quantity} is not positive for ProductId: {ProductId}",
                quantity, product.Id);
            return false;
        }

        var canReserve = product.HasSufficientStock(quantity);
        _logger.LogDebug("CanReserve check for ProductId: {ProductId}, Quantity: {Quantity}, Result: {CanReserve}",
            product.Id, quantity, canReserve);
        return canReserve;
    }

    /// <inheritdoc />
    public ReservationEntity CreateReservation(ProductEntity product, int quantity, int ttlMinutes = 15)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (!CanReserve(product, quantity))
        {
            _logger.LogWarning("CreateReservation failed: Insufficient stock for ProductId: {ProductId}, Requested: {Quantity}",
                product.Id, quantity);
            throw new InvalidOperationException($"Insufficient stock to reserve {quantity} units of product {product.Id}");
        }

        // Create the reservation
        var reservation = ReservationEntity.Create(product.Id, quantity, ttlMinutes);

        // Decrease available stock
        product.RemoveStock(quantity);

        _logger.LogInformation("Reservation created: {ReservationId} for ProductId: {ProductId}, Quantity: {Quantity}, TTL: {TTL} minutes",
            reservation.Id, product.Id, quantity, ttlMinutes);

        return reservation;
    }

    /// <inheritdoc />
    public void CompleteReservation(ReservationEntity reservation)
    {
        ArgumentNullException.ThrowIfNull(reservation);

        if (!reservation.IsValid)
        {
            _logger.LogWarning("CompleteReservation failed: Reservation {ReservationId} is not valid for checkout",
                reservation.Id);
            throw new InvalidOperationException("Reservation is not valid for checkout");
        }

        reservation.Complete();
        _logger.LogInformation("Reservation completed: {ReservationId}", reservation.Id);
    }

    /// <inheritdoc />
    public void CancelReservation(ReservationEntity reservation, ProductEntity product)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(product);

        if (reservation.ProductId != product.Id)
        {
            _logger.LogError("CancelReservation failed: Product mismatch for ReservationId: {ReservationId}, ExpectedProductId: {ExpectedProductId}, ActualProductId: {ActualProductId}",
                reservation.Id, reservation.ProductId, product.Id);
            throw new InvalidOperationException("Reservation product mismatch");
        }

        if (!reservation.Status.CanCancel)
        {
            _logger.LogWarning("CancelReservation failed: Cannot cancel reservation {ReservationId} with status {Status}",
                reservation.Id, reservation.Status);
            throw new InvalidOperationException($"Cannot cancel reservation with status: {reservation.Status}");
        }

        // Restore the stock
        product.AddStock(reservation.Quantity);

        // Mark reservation as cancelled
        reservation.Cancel();
        _logger.LogInformation("Reservation cancelled: {ReservationId}, Stock restored: {Quantity}", reservation.Id, reservation.Quantity);
    }

    /// <inheritdoc />
    public void ExpireReservation(ReservationEntity reservation, ProductEntity product)
    {
        ArgumentNullException.ThrowIfNull(reservation);
        ArgumentNullException.ThrowIfNull(product);

        if (reservation.ProductId != product.Id)
        {
            _logger.LogError("ExpireReservation failed: Product mismatch for ReservationId: {ReservationId}, ExpectedProductId: {ExpectedProductId}, ActualProductId: {ActualProductId}",
                reservation.Id, reservation.ProductId, product.Id);
            throw new InvalidOperationException("Reservation product mismatch");
        }

        if (!reservation.Status.IsActive)
        {
            _logger.LogDebug("ExpireReservation: Reservation {ReservationId} already finalized, skipping", reservation.Id);
            return; // Already finalized, no-op
        }

        // Restore the stock
        product.AddStock(reservation.Quantity);

        // Mark reservation as expired
        reservation.Expire();
        _logger.LogInformation("Reservation expired: {ReservationId}, Stock restored: {Quantity}", reservation.Id, reservation.Quantity);
    }
}
