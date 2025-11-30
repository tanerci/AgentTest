using Microsoft.Extensions.Logging;
using ProductApi.Application.Common;
using ProductApi.Application.DTOs;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;
using ProductApi.Domain.Services;
using ProductApi.Domain.ValueObjects;

namespace ProductApi.Application.Services;

/// <summary>
/// Application service for reservation operations.
/// Orchestrates domain services, repositories, and Redis cache for reservation management.
/// </summary>
public class ReservationService : IReservationService
{
    private readonly IReservationDomainService _domainService;
    private readonly IProductRepository _productRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReservationAuditRepository _auditRepository;
    private readonly IRedisReservationRepository _redisRepository;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationDomainService domainService,
        IProductRepository productRepository,
        IReservationRepository reservationRepository,
        IReservationAuditRepository auditRepository,
        IRedisReservationRepository redisRepository,
        ILogger<ReservationService> logger)
    {
        _domainService = domainService;
        _productRepository = productRepository;
        _reservationRepository = reservationRepository;
        _auditRepository = auditRepository;
        _redisRepository = redisRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<ReservationDto>> ReserveAsync(ReserveRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating reservation for ProductId: {ProductId}, Quantity: {Quantity}, TTL: {TTL} minutes",
            request.ProductId, request.Quantity, request.TtlMinutes);

        // Get the product
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found", request.ProductId);
            return Result.Failure<ReservationDto>(Error.NotFound($"Product with ID {request.ProductId} not found"));
        }

        // Check if we can reserve using domain service
        if (!_domainService.CanReserve(product, request.Quantity))
        {
            _logger.LogWarning("Insufficient stock for ProductId: {ProductId}, Requested: {Quantity}, Available: {Available}",
                request.ProductId, request.Quantity, product.Stock.Quantity);
            return Result.Failure<ReservationDto>(Error.Validation("Insufficient stock available"));
        }

        // Create reservation using domain service
        var reservation = _domainService.CreateReservation(product, request.Quantity, request.TtlMinutes);

        // Attempt Redis reservation atomically
        var ttlSeconds = request.TtlMinutes * 60;
        var redisResult = await _redisRepository.ReserveAsync(
            request.ProductId,
            reservation.Id,
            request.Quantity,
            ttlSeconds);

        if (redisResult != ReservationOperationResult.Reserved)
        {
            // Redis reservation failed - this could mean insufficient stock in cache
            // We need to sync with the actual database state
            _logger.LogWarning("Redis reservation failed: {Result}", redisResult);
            return Result.Failure<ReservationDto>(Error.Validation("Unable to reserve stock - insufficient availability"));
        }

        // Persist reservation to SQL
        await _reservationRepository.AddAsync(reservation, cancellationToken);

        // Update product stock in SQL
        await _productRepository.UpdateAsync(product, cancellationToken);

        // Create audit record
        var audit = ReservationAuditEntity.Create(reservation, "Reservation created");
        await _auditRepository.AddAsync(audit, cancellationToken);

        _logger.LogInformation("Reservation created: {ReservationId} for ProductId: {ProductId}",
            reservation.Id, request.ProductId);

        return Result.Success(MapToDto(reservation));
    }

    /// <inheritdoc />
    public async Task<Result<ReservationDto>> CheckoutAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing checkout for ReservationId: {ReservationId}", request.ReservationId);

        var reservationId = ReservationId.TryParse(request.ReservationId);
        if (reservationId == null)
        {
            return Result.Failure<ReservationDto>(Error.Validation("Invalid reservation ID format"));
        }

        // Get the reservation
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation == null)
        {
            _logger.LogWarning("Reservation not found: {ReservationId}", request.ReservationId);
            return Result.Failure<ReservationDto>(Error.NotFound("Reservation not found"));
        }

        // Check if reservation is valid for checkout
        if (!reservation.IsValid)
        {
            var errorMessage = reservation.IsExpired
                ? "Reservation has expired"
                : $"Reservation cannot be checked out with status: {reservation.Status}";
            _logger.LogWarning("Invalid reservation for checkout: {ReservationId}, Status: {Status}, Expired: {Expired}",
                reservation.Id, reservation.Status, reservation.IsExpired);
            return Result.Failure<ReservationDto>(Error.Validation(errorMessage));
        }

        // Process Redis checkout
        var redisResult = await _redisRepository.CheckoutAsync(reservationId);
        if (redisResult != ReservationOperationResult.CheckedOut)
        {
            _logger.LogWarning("Redis checkout failed: {Result}", redisResult);
            return Result.Failure<ReservationDto>(Error.Failure("Failed to complete checkout"));
        }

        // Complete reservation using domain service
        _domainService.CompleteReservation(reservation);

        // Update reservation in SQL
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);

        // Create audit record
        var audit = ReservationAuditEntity.Create(reservation, "Reservation completed (checked out)");
        await _auditRepository.AddAsync(audit, cancellationToken);

        _logger.LogInformation("Checkout completed for ReservationId: {ReservationId}", request.ReservationId);

        return Result.Success(MapToDto(reservation));
    }

    /// <inheritdoc />
    public async Task<Result<ReservationDto>> CancelAsync(string reservationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing cancellation for ReservationId: {ReservationId}", reservationId);

        var parsedId = ReservationId.TryParse(reservationId);
        if (parsedId == null)
        {
            return Result.Failure<ReservationDto>(Error.Validation("Invalid reservation ID format"));
        }

        // Get the reservation
        var reservation = await _reservationRepository.GetByIdAsync(parsedId, cancellationToken);
        if (reservation == null)
        {
            _logger.LogWarning("Reservation not found: {ReservationId}", reservationId);
            return Result.Failure<ReservationDto>(Error.NotFound("Reservation not found"));
        }

        // Check if reservation can be cancelled
        if (!reservation.Status.CanCancel)
        {
            _logger.LogWarning("Reservation cannot be cancelled: {ReservationId}, Status: {Status}",
                reservation.Id, reservation.Status);
            return Result.Failure<ReservationDto>(Error.Validation($"Cannot cancel reservation with status: {reservation.Status}"));
        }

        // Get the product to restore stock
        var product = await _productRepository.GetByIdAsync(reservation.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogError("Product not found for reservation: {ReservationId}, ProductId: {ProductId}",
                reservation.Id, reservation.ProductId);
            return Result.Failure<ReservationDto>(Error.Failure("Product not found"));
        }

        // Release in Redis
        var redisResult = await _redisRepository.ReleaseAsync(parsedId);
        if (redisResult != ReservationOperationResult.Cancelled && redisResult != ReservationOperationResult.ReservationNotFound)
        {
            _logger.LogWarning("Redis release failed: {Result}", redisResult);
            // Continue with SQL updates even if Redis fails - data integrity in SQL is primary
        }

        // Cancel reservation using domain service
        _domainService.CancelReservation(reservation, product);

        // Update reservation and product in SQL
        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _productRepository.UpdateAsync(product, cancellationToken);

        // Create audit record
        var audit = ReservationAuditEntity.Create(reservation, "Reservation cancelled");
        await _auditRepository.AddAsync(audit, cancellationToken);

        _logger.LogInformation("Reservation cancelled: {ReservationId}", reservationId);

        return Result.Success(MapToDto(reservation));
    }

    /// <inheritdoc />
    public async Task<Result<ReservationDto>> GetByIdAsync(string reservationId, CancellationToken cancellationToken = default)
    {
        var parsedId = ReservationId.TryParse(reservationId);
        if (parsedId == null)
        {
            return Result.Failure<ReservationDto>(Error.Validation("Invalid reservation ID format"));
        }

        var reservation = await _reservationRepository.GetByIdAsync(parsedId, cancellationToken);
        if (reservation == null)
        {
            return Result.Failure<ReservationDto>(Error.NotFound("Reservation not found"));
        }

        return Result.Success(MapToDto(reservation));
    }

    /// <inheritdoc />
    public async Task<int> ProcessExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing expired reservations");

        var expiredReservations = await _reservationRepository.GetExpiredReservationsAsync(cancellationToken);
        var count = 0;

        foreach (var reservation in expiredReservations)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(reservation.ProductId, cancellationToken);
                if (product == null)
                {
                    _logger.LogError("Product not found for expired reservation: {ReservationId}", reservation.Id);
                    continue;
                }

                // Release in Redis
                await _redisRepository.ReleaseAsync(reservation.Id);

                // Expire reservation using domain service
                _domainService.ExpireReservation(reservation, product);

                // Update in SQL
                await _reservationRepository.UpdateAsync(reservation, cancellationToken);
                await _productRepository.UpdateAsync(product, cancellationToken);

                // Create audit record
                var audit = ReservationAuditEntity.Create(reservation, "Reservation expired");
                await _auditRepository.AddAsync(audit, cancellationToken);

                count++;
                _logger.LogInformation("Expired reservation: {ReservationId}", reservation.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired reservation: {ReservationId}", reservation.Id);
            }
        }

        _logger.LogInformation("Processed {Count} expired reservations", count);
        return count;
    }

    private static ReservationDto MapToDto(ReservationEntity entity)
    {
        return new ReservationDto
        {
            ReservationId = entity.Id.ToString(),
            ProductId = entity.ProductId,
            Quantity = entity.Quantity,
            Status = entity.Status.ToString(),
            CreatedAt = entity.CreatedAt,
            ExpiresAt = entity.ExpiresAt,
            CompletedAt = entity.CompletedAt
        };
    }
}
