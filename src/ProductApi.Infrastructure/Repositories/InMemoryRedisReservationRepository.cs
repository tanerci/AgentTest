using System.Collections.Concurrent;
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

    /// <inheritdoc />
    public Task<ReservationOperationResult> ReserveAsync(
        int productId,
        ReservationId reservationId,
        int quantity,
        int ttlSeconds)
    {
        lock (_lockObject)
        {
            // Check available stock
            if (!_availableStock.TryGetValue(productId, out var available))
            {
                return Task.FromResult(ReservationOperationResult.InsufficientStock);
            }

            if (available < quantity)
            {
                return Task.FromResult(ReservationOperationResult.InsufficientStock);
            }

            // Atomically update stock
            _availableStock[productId] = available - quantity;
            _reservedStock.AddOrUpdate(productId, quantity, (_, existing) => existing + quantity);

            // Store reservation with TTL
            var expiresAt = DateTime.UtcNow.AddSeconds(ttlSeconds);
            _reservations[reservationId.Value] = (productId, quantity, expiresAt);

            return Task.FromResult(ReservationOperationResult.Reserved);
        }
    }

    /// <inheritdoc />
    public Task<ReservationOperationResult> CheckoutAsync(ReservationId reservationId)
    {
        lock (_lockObject)
        {
            if (!_reservations.TryRemove(reservationId.Value, out var reservation))
            {
                return Task.FromResult(ReservationOperationResult.ReservationNotFound);
            }

            // Check if expired
            if (DateTime.UtcNow > reservation.ExpiresAt)
            {
                // Reservation expired - restore stock and return error
                _availableStock.AddOrUpdate(reservation.ProductId, reservation.Quantity, (_, existing) => existing + reservation.Quantity);
                _reservedStock.AddOrUpdate(reservation.ProductId, 0, (_, existing) => Math.Max(0, existing - reservation.Quantity));
                return Task.FromResult(ReservationOperationResult.Expired);
            }

            // Decrement reserved stock (stock is now permanently removed)
            _reservedStock.AddOrUpdate(reservation.ProductId, 0, (_, existing) => Math.Max(0, existing - reservation.Quantity));

            return Task.FromResult(ReservationOperationResult.CheckedOut);
        }
    }

    /// <inheritdoc />
    public Task<ReservationOperationResult> ReleaseAsync(ReservationId reservationId)
    {
        lock (_lockObject)
        {
            if (!_reservations.TryRemove(reservationId.Value, out var reservation))
            {
                return Task.FromResult(ReservationOperationResult.ReservationNotFound);
            }

            // Restore available stock
            _availableStock.AddOrUpdate(reservation.ProductId, reservation.Quantity, (_, existing) => existing + reservation.Quantity);

            // Decrement reserved stock
            _reservedStock.AddOrUpdate(reservation.ProductId, 0, (_, existing) => Math.Max(0, existing - reservation.Quantity));

            return Task.FromResult(ReservationOperationResult.Cancelled);
        }
    }

    /// <inheritdoc />
    public Task<int?> GetAvailableStockAsync(int productId)
    {
        if (_availableStock.TryGetValue(productId, out var stock))
        {
            return Task.FromResult<int?>(stock);
        }
        return Task.FromResult<int?>(null);
    }

    /// <inheritdoc />
    public Task InitializeStockAsync(int productId, int availableStock)
    {
        lock (_lockObject)
        {
            _availableStock[productId] = availableStock;
            _reservedStock.TryAdd(productId, 0);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<(int ProductId, int Quantity)?> GetReservationAsync(ReservationId reservationId)
    {
        if (_reservations.TryGetValue(reservationId.Value, out var reservation))
        {
            return Task.FromResult<(int ProductId, int Quantity)?>(
                (reservation.ProductId, reservation.Quantity));
        }
        return Task.FromResult<(int ProductId, int Quantity)?>(null);
    }
}
