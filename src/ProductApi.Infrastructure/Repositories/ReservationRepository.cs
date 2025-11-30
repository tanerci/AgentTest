using Microsoft.EntityFrameworkCore;
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

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ReservationEntity?> GetByIdAsync(ReservationId id, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id.Value, cancellationToken);

        return reservation == null ? null : MapToDomain(reservation);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReservationEntity>> GetActiveByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == "Reserved")
            .ToListAsync(cancellationToken);

        return reservations.Select(MapToDomain);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReservationEntity>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var reservations = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Status == "Reserved" && r.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        return reservations.Select(MapToDomain);
    }

    /// <inheritdoc />
    public async Task<ReservationEntity> AddAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        var model = MapToModel(reservation);
        _context.Reservations.Add(model);
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDomain(model);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ReservationEntity reservation, CancellationToken cancellationToken = default)
    {
        var model = await _context.Reservations.FindAsync([reservation.Id.Value], cancellationToken);
        if (model == null)
            throw new InvalidOperationException($"Reservation with ID {reservation.Id} not found");

        model.Status = reservation.Status.ToString();
        model.CompletedAt = reservation.CompletedAt;

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(ReservationId id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations.AnyAsync(r => r.Id == id.Value, cancellationToken);
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
