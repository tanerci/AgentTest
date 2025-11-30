using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Application.DTOs;
using ProductApi.Application.Services;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Repositories;
using ProductApi.Domain.Services;
using ProductApi.Domain.ValueObjects;
using ProductApi.Infrastructure.Persistence;
using ProductApi.Infrastructure.Repositories;
using Xunit;

namespace ProductApi.Tests;

/// <summary>
/// Tests for the Reservation Service functionality.
/// </summary>
public class ReservationServiceTests : TestBase
{
    private readonly IReservationDomainService _domainService;
    private readonly ILogger<ReservationService> _logger;
    private readonly ILogger<ReservationDomainService> _domainServiceLogger;
    private readonly ILogger<ReservationRepository> _reservationRepoLogger;
    private readonly ILogger<ReservationAuditRepository> _auditRepoLogger;
    private readonly ILogger<InMemoryRedisReservationRepository> _redisRepoLogger;

    public ReservationServiceTests()
    {
        _domainServiceLogger = Substitute.For<ILogger<ReservationDomainService>>();
        _domainService = new ReservationDomainService(_domainServiceLogger);
        _logger = Substitute.For<ILogger<ReservationService>>();
        _reservationRepoLogger = Substitute.For<ILogger<ReservationRepository>>();
        _auditRepoLogger = Substitute.For<ILogger<ReservationAuditRepository>>();
        _redisRepoLogger = Substitute.For<ILogger<InMemoryRedisReservationRepository>>();
    }

    private IReservationService GetReservationService(
        AppDbContext context,
        IRedisReservationRepository? redisRepo = null)
    {
        var productRepo = new ProductRepository(context);
        var reservationRepo = new ReservationRepository(context, _reservationRepoLogger);
        var auditRepo = new ReservationAuditRepository(context, _auditRepoLogger);
        var redis = redisRepo ?? new InMemoryRedisReservationRepository(_redisRepoLogger);

        return new ReservationService(
            _domainService,
            productRepo,
            reservationRepo,
            auditRepo,
            redis,
            _logger);
    }

    private async Task<IRedisReservationRepository> GetInitializedRedisRepo(AppDbContext context)
    {
        var redisRepo = new InMemoryRedisReservationRepository(_redisRepoLogger);
        foreach (var product in context.Products)
        {
            await redisRepo.InitializeStockAsync(product.Id, product.Stock);
        }
        return redisRepo;
    }

    [Fact]
    public async Task Reserve_WithValidProduct_ReturnsReservation()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        var request = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 3,
            TtlMinutes = 15
        };

        // Act
        var result = await service.ReserveAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(1, result.Value.ProductId);
        Assert.Equal(3, result.Value.Quantity);
        Assert.Equal("Reserved", result.Value.Status);
        Assert.NotEmpty(result.Value.ReservationId);
    }

    [Fact]
    public async Task Reserve_WithInsufficientStock_ReturnsFailure()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 5
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        var request = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 10,
            TtlMinutes = 15
        };

        // Act
        var result = await service.ReserveAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Insufficient", result.Error.Message);
    }

    [Fact]
    public async Task Reserve_WithNonExistentProduct_ReturnsNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        var request = new ReserveRequest
        {
            ProductId = 999,
            Quantity = 1,
            TtlMinutes = 15
        };

        // Act
        var result = await service.ReserveAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Checkout_WithValidReservation_ReturnsCompleted()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        // Create a reservation first
        var reserveRequest = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 2,
            TtlMinutes = 15
        };
        var reserveResult = await service.ReserveAsync(reserveRequest);
        Assert.True(reserveResult.IsSuccess);

        var checkoutRequest = new CheckoutRequest
        {
            ReservationId = reserveResult.Value.ReservationId
        };

        // Act
        var result = await service.CheckoutAsync(checkoutRequest);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Completed", result.Value.Status);
        Assert.NotNull(result.Value.CompletedAt);
    }

    [Fact]
    public async Task Checkout_WithNonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        var checkoutRequest = new CheckoutRequest
        {
            ReservationId = Guid.NewGuid().ToString()
        };

        // Act
        var result = await service.CheckoutAsync(checkoutRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Checkout_WithInvalidReservationIdFormat_ReturnsValidationError()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        var checkoutRequest = new CheckoutRequest
        {
            ReservationId = "invalid-guid"
        };

        // Act
        var result = await service.CheckoutAsync(checkoutRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task Cancel_WithActiveReservation_ReturnsSuccess()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        // Create a reservation first
        var reserveRequest = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 3,
            TtlMinutes = 15
        };
        var reserveResult = await service.ReserveAsync(reserveRequest);
        Assert.True(reserveResult.IsSuccess);

        // Act
        var result = await service.CancelAsync(reserveResult.Value.ReservationId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Cancelled", result.Value.Status);
    }

    [Fact]
    public async Task Cancel_WithNonExistentReservation_ReturnsNotFound()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        // Act
        var result = await service.CancelAsync(Guid.NewGuid().ToString());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task GetById_WithExistingReservation_ReturnsReservation()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        // Create a reservation first
        var reserveRequest = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 2,
            TtlMinutes = 15
        };
        var reserveResult = await service.ReserveAsync(reserveRequest);
        Assert.True(reserveResult.IsSuccess);

        // Act
        var result = await service.GetByIdAsync(reserveResult.Value.ReservationId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(reserveResult.Value.ReservationId, result.Value.ReservationId);
        Assert.Equal(1, result.Value.ProductId);
        Assert.Equal(2, result.Value.Quantity);
    }

    [Fact]
    public async Task Reserve_DecreasesAvailableStock()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        var request = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 4,
            TtlMinutes = 15
        };

        // Act
        await service.ReserveAsync(request);

        // Assert - check Redis stock was decreased
        var remainingStock = await redisRepo.GetAvailableStockAsync(1);
        Assert.Equal(6, remainingStock);
    }

    [Fact]
    public async Task Cancel_RestoresAvailableStock()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        // Reserve some stock
        var reserveRequest = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 4,
            TtlMinutes = 15
        };
        var reserveResult = await service.ReserveAsync(reserveRequest);
        Assert.True(reserveResult.IsSuccess);

        // Check stock was decreased
        var stockAfterReserve = await redisRepo.GetAvailableStockAsync(1);
        Assert.Equal(6, stockAfterReserve);

        // Act - cancel the reservation
        await service.CancelAsync(reserveResult.Value.ReservationId);

        // Assert - stock should be restored
        var stockAfterCancel = await redisRepo.GetAvailableStockAsync(1);
        Assert.Equal(10, stockAfterCancel);
    }

    [Fact]
    public async Task Checkout_DoesNotRestoreStock()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        // Reserve some stock
        var reserveRequest = new ReserveRequest
        {
            ProductId = 1,
            Quantity = 4,
            TtlMinutes = 15
        };
        var reserveResult = await service.ReserveAsync(reserveRequest);
        Assert.True(reserveResult.IsSuccess);

        // Check stock was decreased
        var stockAfterReserve = await redisRepo.GetAvailableStockAsync(1);
        Assert.Equal(6, stockAfterReserve);

        // Act - checkout the reservation
        var checkoutRequest = new CheckoutRequest
        {
            ReservationId = reserveResult.Value.ReservationId
        };
        await service.CheckoutAsync(checkoutRequest);

        // Assert - stock should remain at 6 (not restored)
        var stockAfterCheckout = await redisRepo.GetAvailableStockAsync(1);
        Assert.Equal(6, stockAfterCheckout);
    }

    [Fact]
    public async Task MultipleReservations_DecreasesStockCorrectly()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.Products.Add(new Infrastructure.Persistence.Models.Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 20
        });
        await context.SaveChangesAsync();

        var redisRepo = await GetInitializedRedisRepo(context);
        var service = GetReservationService(context, redisRepo);

        // Act - create multiple reservations
        await service.ReserveAsync(new ReserveRequest { ProductId = 1, Quantity = 5, TtlMinutes = 15 });
        await service.ReserveAsync(new ReserveRequest { ProductId = 1, Quantity = 3, TtlMinutes = 15 });
        await service.ReserveAsync(new ReserveRequest { ProductId = 1, Quantity = 7, TtlMinutes = 15 });

        // Assert - 5 + 3 + 7 = 15 reserved, so 5 available
        var remainingStock = await redisRepo.GetAvailableStockAsync(1);
        Assert.Equal(5, remainingStock);

        // Fourth reservation should fail if trying to reserve more than available
        var failedResult = await service.ReserveAsync(new ReserveRequest { ProductId = 1, Quantity = 10, TtlMinutes = 15 });
        Assert.False(failedResult.IsSuccess);
    }
}
