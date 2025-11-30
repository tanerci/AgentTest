using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;
using ProductApi.Domain.ValueObjects;
using ProductApi.Infrastructure.Persistence;
using ProductApi.Infrastructure.Persistence.Models;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ReservationAudit persistence.
/// </summary>
public class ReservationAuditRepository : IReservationAuditRepository
{
    private readonly AppDbContext _context;

    public ReservationAuditRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ReservationAuditEntity> AddAsync(ReservationAuditEntity audit, CancellationToken cancellationToken = default)
    {
        var model = MapToModel(audit);
        _context.ReservationAudits.Add(model);
        await _context.SaveChangesAsync(cancellationToken);
        return MapToDomain(model);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReservationAuditEntity>> GetByReservationIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default)
    {
        var audits = await _context.ReservationAudits
            .AsNoTracking()
            .Where(a => a.ReservationId == reservationId.Value)
            .OrderBy(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        return audits.Select(MapToDomain);
    }

    private static ReservationAuditEntity MapToDomain(ReservationAudit model)
    {
        return ReservationAuditEntity.Hydrate(
            model.Id,
            model.ReservationId,
            model.ProductId,
            model.Quantity,
            model.Status,
            model.Timestamp,
            model.Notes);
    }

    private static ReservationAudit MapToModel(ReservationAuditEntity entity)
    {
        return new ReservationAudit
        {
            ReservationId = entity.ReservationId.Value,
            ProductId = entity.ProductId,
            Quantity = entity.Quantity,
            Status = entity.Status.ToString(),
            Timestamp = entity.Timestamp,
            Notes = entity.Notes
        };
    }
}
