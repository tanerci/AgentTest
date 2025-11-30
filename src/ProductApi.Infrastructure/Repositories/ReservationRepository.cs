using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;
using ProductApi.Domain.ValueObjects;
using ProductApi.Infrastructure.Persistence;
using ProductApi.Infrastructure.Persistence.Models;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Reservation aggregate root.
/// </summary>
public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReservationRepository> _logger;

    public ReservationRepository(AppDbContext context, ILogger<ReservationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReservationEntity?> GetByIdAsync(ReservationId id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching reservation by Id: {ReservationId}", id);

        var reservation = await _context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id.Value, cancellationToken);

        if (reservation == null)
        {
            _logger.LogDebug("Reservation not found: {ReservationId}", id);
            return null;
        }

        _logger.LogDebug("Reservation retrieved: {ReservationId}", id);
        return MapToDomain(reservation);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReservationEntity>> GetActiveByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching active reservations for ProductId: {ProductId}", productId);

        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == "Reserved")
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} active reservations for ProductId: {ProductId}", reservations.Count, productId);
        return reservations.Select(MapToDomain);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReservationEntity>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching expired reservations");

        var now = DateTime.UtcNow;
        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Status == "Reserved" && r.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} expired reservations", reservations.Count);
        return reservations.Select(MapToDomain);
    }

    /// <inheritdoc />
    public async Task<ReservationEntity> AddAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding reservation: {ReservationId}", reservation.Id);

        var model = MapToModel(reservation);
        _context.Reservations.Add(model);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reservation added: {ReservationId} for ProductId: {ProductId}", reservation.Id, reservation.ProductId);
        return MapToDomain(model);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating reservation: {ReservationId}", reservation.Id);

        var model = await _context.Reservations.FindAsync([reservation.Id.Value], cancellationToken);
        if (model == null)
        {
            _logger.LogError("Update failed: Reservation not found: {ReservationId}", reservation.Id);
            throw new InvalidOperationException($"Reservation with ID {reservation.Id} not found");
        }

        model.Status = reservation.Status.ToString();
        model.CompletedAt = reservation.CompletedAt;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Reservation updated: {ReservationId}, Status: {Status}", reservation.Id, reservation.Status);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(ReservationId id, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Reservations.AnyAsync(r => r.Id == id.Value, cancellationToken);
        _logger.LogDebug("Reservation exists check: {ReservationId}, Exists: {Exists}", id, exists);
        return exists;
    }

    private static ReservationEntity MapToDomain(Reservation model)
    {
        return ReservationEntity.Hydrate(
            model.Id,
            model.ProductId,
            model.Quantity,
            model.Status,
            model.CreatedAt,
            model.ExpiresAt,
            model.CompletedAt);
    }

    private static Reservation MapToModel(ReservationEntity entity)
    {
        return new Reservation
        {
            Id = entity.Id.Value,
            ProductId = entity.ProductId,
            Quantity = entity.Quantity,
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            ExpiresAt = entity.ExpiresAt,
            CompletedAt = entity.CompletedAt
        };
    }
}
