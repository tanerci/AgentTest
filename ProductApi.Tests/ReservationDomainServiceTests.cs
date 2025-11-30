using Microsoft.Extensions.Logging;
using NSubstitute;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Services;
using ProductApi.Domain.ValueObjects;
using Xunit;

namespace ProductApi.Tests;

/// <summary>
/// Tests for the Reservation Domain Service.
/// </summary>
public class ReservationDomainServiceTests : TestBase
{
    private readonly ReservationDomainService _domainService;

    public ReservationDomainServiceTests()
    {
        var logger = Substitute.For<ILogger<ReservationDomainService>>();
        _domainService = new ReservationDomainService(logger);
    }

    [Fact]
    public void CanReserve_WithSufficientStock_ReturnsTrue()
    {
        // Arrange
        var product = ProductEntity.Create("Test Product", "Description", 99.99m, 10);

        // Act
        var result = _domainService.CanReserve(product, 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanReserve_WithInsufficientStock_ReturnsFalse()
    {
        // Arrange
        var product = ProductEntity.Create("Test Product", "Description", 99.99m, 5);

        // Act
        var result = _domainService.CanReserve(product, 10);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanReserve_WithExactStock_ReturnsTrue()
    {
        // Arrange
        var product = ProductEntity.Create("Test Product", "Description", 99.99m, 5);

        // Act
        var result = _domainService.CanReserve(product, 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanReserve_WithZeroQuantity_ReturnsFalse()
    {
        // Arrange
        var product = ProductEntity.Create("Test Product", "Description", 99.99m, 10);

        // Act
        var result = _domainService.CanReserve(product, 0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanReserve_WithNegativeQuantity_ReturnsFalse()
    {
        // Arrange
        var product = ProductEntity.Create("Test Product", "Description", 99.99m, 10);

        // Act
        var result = _domainService.CanReserve(product, -5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CreateReservation_WithValidInput_ReturnsReservation()
    {
        // Arrange - Use Hydrate to create a product with a valid ID
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);

        // Act
        var reservation = _domainService.CreateReservation(product, 3, 15);

        // Assert
        Assert.NotNull(reservation);
        Assert.Equal(3, reservation.Quantity);
        Assert.Equal(ReservationStatus.Reserved, reservation.Status);
        Assert.True(reservation.IsValid);
    }

    [Fact]
    public void CreateReservation_DecreasesProductStock()
    {
        // Arrange - Use Hydrate to create a product with a valid ID
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var initialStock = product.Stock.Quantity;

        // Act
        _domainService.CreateReservation(product, 3, 15);

        // Assert
        Assert.Equal(initialStock - 3, product.Stock.Quantity);
    }

    [Fact]
    public void CreateReservation_WithInsufficientStock_ThrowsException()
    {
        // Arrange - Use Hydrate to create a product with a valid ID
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 5);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            _domainService.CreateReservation(product, 10, 15));
    }

    [Fact]
    public void CompleteReservation_WithValidReservation_SetsStatusToCompleted()
    {
        // Arrange - Use Hydrate to create a product with a valid ID
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);

        // Act
        _domainService.CompleteReservation(reservation);

        // Assert
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
        Assert.NotNull(reservation.CompletedAt);
    }

    [Fact]
    public void CancelReservation_RestoresProductStock()
    {
        // Arrange - Use Hydrate to create a product with a valid ID
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);
        var stockAfterReserve = product.Stock.Quantity;

        // Act
        _domainService.CancelReservation(reservation, product);

        // Assert
        Assert.Equal(stockAfterReserve + 3, product.Stock.Quantity);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void ExpireReservation_RestoresProductStock()
    {
        // Arrange - Use Hydrate to create a product with a valid ID
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);
        var stockAfterReserve = product.Stock.Quantity;

        // Act
        _domainService.ExpireReservation(reservation, product);

        // Assert
        Assert.Equal(stockAfterReserve + 3, product.Stock.Quantity);
        Assert.Equal(ReservationStatus.Expired, reservation.Status);
    }

    [Fact]
    public void CancelReservation_WithMismatchedProduct_ThrowsException()
    {
        // Arrange - Use Hydrate to create products with different valid IDs
        var product1 = ProductEntity.Hydrate(1, "Test Product 1", "Description", 99.99m, 10);
        var product2 = ProductEntity.Hydrate(2, "Test Product 2", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product1, 3, 15);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            _domainService.CancelReservation(reservation, product2));
    }

    [Fact]
    public void CancelReservation_WithNonCancellableStatus_ThrowsException()
    {
        // Arrange
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);
        
        // Complete the reservation first
        _domainService.CompleteReservation(reservation);

        // Act & Assert - Cannot cancel a completed reservation
        Assert.Throws<InvalidOperationException>(() => 
            _domainService.CancelReservation(reservation, product));
    }

    [Fact]
    public void ExpireReservation_WithMismatchedProduct_ThrowsException()
    {
        // Arrange
        var product1 = ProductEntity.Hydrate(1, "Test Product 1", "Description", 99.99m, 10);
        var product2 = ProductEntity.Hydrate(2, "Test Product 2", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product1, 3, 15);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            _domainService.ExpireReservation(reservation, product2));
    }

    [Fact]
    public void ExpireReservation_WithAlreadyFinalizedReservation_DoesNothing()
    {
        // Arrange
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);
        var stockAfterReserve = product.Stock.Quantity;
        
        // Complete the reservation first
        _domainService.CompleteReservation(reservation);

        // Act - Try to expire a completed reservation (should be no-op)
        _domainService.ExpireReservation(reservation, product);

        // Assert - Status should remain Completed and stock should not change
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
        Assert.Equal(stockAfterReserve, product.Stock.Quantity); // Stock not restored
    }

    [Fact]
    public void CompleteReservation_WithInvalidReservation_ThrowsException()
    {
        // Arrange
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);
        
        // Cancel the reservation first
        _domainService.CancelReservation(reservation, product);

        // Act & Assert - Cannot complete a cancelled reservation
        Assert.Throws<InvalidOperationException>(() => 
            _domainService.CompleteReservation(reservation));
    }

    [Fact]
    public void CanReserve_WithNullProduct_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _domainService.CanReserve(null!, 5));
    }

    [Fact]
    public void CreateReservation_WithNullProduct_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _domainService.CreateReservation(null!, 5, 15));
    }

    [Fact]
    public void CompleteReservation_WithNullReservation_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _domainService.CompleteReservation(null!));
    }

    [Fact]
    public void CancelReservation_WithNullReservation_ThrowsArgumentNullException()
    {
        // Arrange
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _domainService.CancelReservation(null!, product));
    }

    [Fact]
    public void CancelReservation_WithNullProduct_ThrowsArgumentNullException()
    {
        // Arrange
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _domainService.CancelReservation(reservation, null!));
    }

    [Fact]
    public void ExpireReservation_WithNullReservation_ThrowsArgumentNullException()
    {
        // Arrange
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _domainService.ExpireReservation(null!, product));
    }

    [Fact]
    public void ExpireReservation_WithNullProduct_ThrowsArgumentNullException()
    {
        // Arrange
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 99.99m, 10);
        var reservation = _domainService.CreateReservation(product, 3, 15);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _domainService.ExpireReservation(reservation, null!));
    }
}
