using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ReservationAuditRepository> _logger;

    public ReservationAuditRepository(AppDbContext context, ILogger<ReservationAuditRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReservationAuditEntity> AddAsync(ReservationAuditEntity audit, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Adding audit record for ReservationId: {ReservationId}, Status: {Status}",
            audit.ReservationId, audit.Status);

        var model = MapToModel(audit);
        _context.ReservationAudits.Add(model);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Audit record added: ReservationId: {ReservationId}, Status: {Status}, Notes: {Notes}",
            audit.ReservationId, audit.Status, audit.Notes);
        return MapToDomain(model);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReservationAuditEntity>> GetByReservationIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching audit records for ReservationId: {ReservationId}", reservationId);

        var audits = await _context.ReservationAudits
            .AsNoTracking()
            .Where(a => a.ReservationId == reservationId.Value)
            .OrderBy(a => a.Timestamp)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Found {Count} audit records for ReservationId: {ReservationId}", audits.Count, reservationId);
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
