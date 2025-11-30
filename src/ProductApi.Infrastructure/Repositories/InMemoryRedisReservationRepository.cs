using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ProductApi.Domain.Repositories;
using ProductApi.Domain.ValueObjects;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IRedisReservationRepository for development/testing.
/// In production, this would be replaced with an actual Redis implementation using StackExchange.Redis.
/// </summary>
/// <remarks>
/// This implementation simulates the atomic operations that would be performed by Redis Lua scripts:
/// - Reserve: DECR available stock, INCR reserved stock, SET TTL key
/// - Checkout: DECR reserved stock, DEL TTL key
/// - Release: INCR available stock, DECR reserved stock, DEL TTL key
/// </remarks>
public class InMemoryRedisReservationRepository : IRedisReservationRepository
{
    private readonly ConcurrentDictionary<int, int> _availableStock = new();
    private readonly ConcurrentDictionary<int, int> _reservedStock = new();
    private readonly ConcurrentDictionary<Guid, (int ProductId, int Quantity, DateTime ExpiresAt)> _reservations = new();
    private readonly object _lockObject = new();
    private readonly ILogger<InMemoryRedisReservationRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the InMemoryRedisReservationRepository.
    /// </summary>
    /// <param name="logger">The logger for Redis repository operations.</param>
    public InMemoryRedisReservationRepository(ILogger<InMemoryRedisReservationRepository> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ReservationOperationResult> ReserveAsync(
        int productId,
        ReservationId reservationId,
        int quantity,
        int ttlSeconds)
    {
        _logger.LogDebug("Redis ReserveAsync: ProductId: {ProductId}, ReservationId: {ReservationId}, Quantity: {Quantity}, TTL: {TTL}s",
            productId, reservationId, quantity, ttlSeconds);

        lock (_lockObject)
        {
            // Check available stock
            if (!_availableStock.TryGetValue(productId, out var available))
            {
                _logger.LogWarning("Redis ReserveAsync: ProductId {ProductId} not found in stock cache", productId);
                return Task.FromResult(ReservationOperationResult.InsufficientStock);
            }

            if (available < quantity)
            {
                _logger.LogWarning("Redis ReserveAsync: Insufficient stock for ProductId: {ProductId}, Available: {Available}, Requested: {Quantity}",
                    productId, available, quantity);
                return Task.FromResult(ReservationOperationResult.InsufficientStock);
            }

            // Atomically update stock
            _availableStock[productId] = available - quantity;
            _reservedStock.AddOrUpdate(productId, quantity, (_, existing) => existing + quantity);

            // Store reservation with TTL
            var expiresAt = DateTime.UtcNow.AddSeconds(ttlSeconds);
            _reservations[reservationId.Value] = (productId, quantity, expiresAt);

            _logger.LogInformation("Redis ReserveAsync: Reserved successfully - ProductId: {ProductId}, ReservationId: {ReservationId}, NewAvailable: {Available}",
                productId, reservationId, available - quantity);
            return Task.FromResult(ReservationOperationResult.Reserved);
        }
    }

    /// <inheritdoc />
    public Task<ReservationOperationResult> CheckoutAsync(ReservationId reservationId)
    {
        _logger.LogDebug("Redis CheckoutAsync: ReservationId: {ReservationId}", reservationId);

        lock (_lockObject)
        {
            if (!_reservations.TryRemove(reservationId.Value, out var reservation))
            {
                _logger.LogWarning("Redis CheckoutAsync: Reservation not found: {ReservationId}", reservationId);
                return Task.FromResult(ReservationOperationResult.ReservationNotFound);
            }

            // Check if expired
            if (DateTime.UtcNow > reservation.ExpiresAt)
            {
                _logger.LogWarning("Redis CheckoutAsync: Reservation expired: {ReservationId}", reservationId);
                // Reservation expired - restore stock and return error
                _availableStock.AddOrUpdate(reservation.ProductId, reservation.Quantity, (_, existing) => existing + reservation.Quantity);
                _reservedStock.AddOrUpdate(reservation.ProductId, 0, (_, existing) => Math.Max(0, existing - reservation.Quantity));
                return Task.FromResult(ReservationOperationResult.Expired);
            }

            // Decrement reserved stock (stock is now permanently removed)
            _reservedStock.AddOrUpdate(reservation.ProductId, 0, (_, existing) => Math.Max(0, existing - reservation.Quantity));

            _logger.LogInformation("Redis CheckoutAsync: Checkout completed - ReservationId: {ReservationId}, ProductId: {ProductId}",
                reservationId, reservation.ProductId);
            return Task.FromResult(ReservationOperationResult.CheckedOut);
        }
    }

    /// <inheritdoc />
    public Task<ReservationOperationResult> ReleaseAsync(ReservationId reservationId)
    {
        _logger.LogDebug("Redis ReleaseAsync: ReservationId: {ReservationId}", reservationId);

        lock (_lockObject)
        {
            if (!_reservations.TryRemove(reservationId.Value, out var reservation))
            {
                _logger.LogWarning("Redis ReleaseAsync: Reservation not found: {ReservationId}", reservationId);
                return Task.FromResult(ReservationOperationResult.ReservationNotFound);
            }

            // Restore available stock
            _availableStock.AddOrUpdate(reservation.ProductId, reservation.Quantity, (_, existing) => existing + reservation.Quantity);

            // Decrement reserved stock
            _reservedStock.AddOrUpdate(reservation.ProductId, 0, (_, existing) => Math.Max(0, existing - reservation.Quantity));

            _logger.LogInformation("Redis ReleaseAsync: Released successfully - ReservationId: {ReservationId}, ProductId: {ProductId}, Quantity: {Quantity}",
                reservationId, reservation.ProductId, reservation.Quantity);
            return Task.FromResult(ReservationOperationResult.Cancelled);
        }
    }

    /// <inheritdoc />
    public Task<int?> GetAvailableStockAsync(int productId)
    {
        if (_availableStock.TryGetValue(productId, out var stock))
        {
            _logger.LogDebug("Redis GetAvailableStockAsync: ProductId: {ProductId}, Stock: {Stock}", productId, stock);
            return Task.FromResult<int?>(stock);
        }
        _logger.LogDebug("Redis GetAvailableStockAsync: ProductId {ProductId} not found", productId);
        return Task.FromResult<int?>(null);
    }

    /// <inheritdoc />
    public Task InitializeStockAsync(int productId, int availableStock)
    {
        _logger.LogDebug("Redis InitializeStockAsync: ProductId: {ProductId}, Stock: {Stock}", productId, availableStock);

        lock (_lockObject)
        {
            _availableStock[productId] = availableStock;
            _reservedStock.TryAdd(productId, 0);
        }

        _logger.LogInformation("Redis InitializeStockAsync: Initialized stock for ProductId: {ProductId}, Available: {Stock}",
            productId, availableStock);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<(int ProductId, int Quantity)?> GetReservationAsync(ReservationId reservationId)
    {
        if (_reservations.TryGetValue(reservationId.Value, out var reservation))
        {
            _logger.LogDebug("Redis GetReservationAsync: Found ReservationId: {ReservationId}", reservationId);
            return Task.FromResult<(int ProductId, int Quantity)?>(
                (reservation.ProductId, reservation.Quantity));
        }
        _logger.LogDebug("Redis GetReservationAsync: ReservationId {ReservationId} not found", reservationId);
        return Task.FromResult<(int ProductId, int Quantity)?>(null);
    }
}
