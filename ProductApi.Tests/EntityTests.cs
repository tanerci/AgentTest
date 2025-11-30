using ProductApi.Domain.Entities;
using ProductApi.Domain.ValueObjects;
using Xunit;

namespace ProductApi.Tests;

/// <summary>
/// Tests for domain entities to ensure business rules and entity behavior.
/// </summary>
public class EntityTests
{
    #region ProductEntity Tests

    [Fact]
    public void ProductEntity_Create_WithValidData_CreatesEntity()
    {
        // Act
        var product = ProductEntity.Create("Test Product", "Description", 99.99m, 50);

        // Assert
        Assert.Equal("Test Product", product.Name.Value);
        Assert.Equal("Description", product.Description);
        Assert.Equal(99.99m, product.Price.Amount);
        Assert.Equal(50, product.Stock.Quantity);
        Assert.Equal(0, product.Id); // ID is 0 until persisted
    }

    [Fact]
    public void ProductEntity_Create_WithNullDescription_SetsEmptyDescription()
    {
        // Act
        var product = ProductEntity.Create("Test Product", null!, 50m, 10);

        // Assert
        Assert.Equal(string.Empty, product.Description);
    }

    [Fact]
    public void ProductEntity_Hydrate_ReconstructsEntity()
    {
        // Act
        var product = ProductEntity.Hydrate(1, "Test Product", "Description", 75.50m, 25);

        // Assert
        Assert.Equal(1, product.Id);
        Assert.Equal("Test Product", product.Name.Value);
        Assert.Equal("Description", product.Description);
        Assert.Equal(75.50m, product.Price.Amount);
        Assert.Equal(25, product.Stock.Quantity);
    }

    [Fact]
    public void ProductEntity_Hydrate_WithInvalidData_UsesFallbackValues()
    {
        // Act - hydrate with invalid name (empty string)
        var product = ProductEntity.Hydrate(1, "", "Description", -10m, -5);

        // Assert - should use fallback values
        Assert.Equal("Unknown Product", product.Name.Value);
        Assert.Equal(0m, product.Price.Amount);
        Assert.Equal(0, product.Stock.Quantity);
    }

    [Fact]
    public void ProductEntity_UpdateName_UpdatesName()
    {
        // Arrange
        var product = ProductEntity.Create("Original Name", "Desc", 50m, 10);

        // Act
        product.UpdateName("New Name");

        // Assert
        Assert.Equal("New Name", product.Name.Value);
    }

    [Fact]
    public void ProductEntity_UpdateDescription_UpdatesDescription()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Original", 50m, 10);

        // Act
        product.UpdateDescription("New Description");

        // Assert
        Assert.Equal("New Description", product.Description);
    }

    [Fact]
    public void ProductEntity_UpdateDescription_WithNull_SetsEmptyString()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Original", 50m, 10);

        // Act
        product.UpdateDescription(null!);

        // Assert
        Assert.Equal(string.Empty, product.Description);
    }

    [Fact]
    public void ProductEntity_UpdatePrice_UpdatesPrice()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Desc", 50m, 10);

        // Act
        product.UpdatePrice(75.99m);

        // Assert
        Assert.Equal(75.99m, product.Price.Amount);
    }

    [Fact]
    public void ProductEntity_UpdateStock_UpdatesStock()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Desc", 50m, 10);

        // Act
        product.UpdateStock(25);

        // Assert
        Assert.Equal(25, product.Stock.Quantity);
    }

    [Fact]
    public void ProductEntity_AddStock_IncreasesStock()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Desc", 50m, 10);

        // Act
        product.AddStock(5);

        // Assert
        Assert.Equal(15, product.Stock.Quantity);
    }

    [Fact]
    public void ProductEntity_RemoveStock_DecreasesStock()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Desc", 50m, 10);

        // Act
        product.RemoveStock(3);

        // Assert
        Assert.Equal(7, product.Stock.Quantity);
    }

    [Fact]
    public void ProductEntity_RemoveStock_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Desc", 50m, 5);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => product.RemoveStock(10));
    }

    [Fact]
    public void ProductEntity_HasSufficientStock_WithEnoughStock_ReturnsTrue()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Desc", 50m, 10);

        // Act & Assert
        Assert.True(product.HasSufficientStock(5));
        Assert.True(product.HasSufficientStock(10));
    }

    [Fact]
    public void ProductEntity_HasSufficientStock_WithInsufficientStock_ReturnsFalse()
    {
        // Arrange
        var product = ProductEntity.Create("Product", "Desc", 50m, 5);

        // Act & Assert
        Assert.False(product.HasSufficientStock(10));
    }

    #endregion

    #region ReservationEntity Tests

    [Fact]
    public void ReservationEntity_Create_WithValidData_CreatesEntity()
    {
        // Act
        var reservation = ReservationEntity.Create(1, 5, 15);

        // Assert
        Assert.Equal(1, reservation.ProductId);
        Assert.Equal(5, reservation.Quantity);
        Assert.Equal(ReservationStatus.Reserved, reservation.Status);
        Assert.True(reservation.IsValid);
        Assert.False(reservation.IsExpired);
        Assert.Null(reservation.CompletedAt);
    }

    [Fact]
    public void ReservationEntity_Create_SetsCorrectExpiresAt()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;

        // Act
        var reservation = ReservationEntity.Create(1, 5, 30);

        // Assert
        Assert.True(reservation.CreatedAt >= beforeCreate);
        Assert.True(reservation.ExpiresAt >= reservation.CreatedAt.AddMinutes(29));
        Assert.True(reservation.ExpiresAt <= reservation.CreatedAt.AddMinutes(31));
    }

    [Fact]
    public void ReservationEntity_Create_WithZeroProductId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationEntity.Create(0, 5, 15));
        Assert.Contains("Product ID", exception.Message);
    }

    [Fact]
    public void ReservationEntity_Create_WithNegativeProductId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationEntity.Create(-1, 5, 15));
        Assert.Contains("Product ID", exception.Message);
    }

    [Fact]
    public void ReservationEntity_Create_WithZeroQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationEntity.Create(1, 0, 15));
        Assert.Contains("Quantity", exception.Message);
    }

    [Fact]
    public void ReservationEntity_Create_WithNegativeQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationEntity.Create(1, -5, 15));
        Assert.Contains("Quantity", exception.Message);
    }

    [Fact]
    public void ReservationEntity_Create_WithZeroTtl_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationEntity.Create(1, 5, 0));
        Assert.Contains("TTL", exception.Message);
    }

    [Fact]
    public void ReservationEntity_Create_WithNegativeTtl_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationEntity.Create(1, 5, -15));
        Assert.Contains("TTL", exception.Message);
    }

    [Fact]
    public void ReservationEntity_Complete_WithValidReservation_SetsCompletedStatus()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);

        // Act
        reservation.Complete();

        // Assert
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
        Assert.NotNull(reservation.CompletedAt);
    }

    [Fact]
    public void ReservationEntity_Complete_WithNonReservedStatus_ThrowsException()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);
        reservation.Cancel();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => reservation.Complete());
    }

    [Fact]
    public void ReservationEntity_Cancel_WithActiveReservation_SetsCancelledStatus()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);

        // Act
        reservation.Cancel();

        // Assert
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
        Assert.NotNull(reservation.CompletedAt);
    }

    [Fact]
    public void ReservationEntity_Cancel_WithCompletedReservation_ThrowsException()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);
        reservation.Complete();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => reservation.Cancel());
    }

    [Fact]
    public void ReservationEntity_Expire_WithActiveReservation_SetsExpiredStatus()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);

        // Act
        reservation.Expire();

        // Assert
        Assert.Equal(ReservationStatus.Expired, reservation.Status);
        Assert.NotNull(reservation.CompletedAt);
    }

    [Fact]
    public void ReservationEntity_Expire_WithAlreadyCompleted_DoesNothing()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);
        reservation.Complete();
        var completedAt = reservation.CompletedAt;

        // Act
        reservation.Expire();

        // Assert - status should remain completed (no-op)
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
    }

    [Fact]
    public void ReservationEntity_IsValid_WithActiveAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 60); // 60 minutes TTL

        // Act & Assert
        Assert.True(reservation.IsValid);
    }

    [Fact]
    public void ReservationEntity_IsValid_WithCancelledStatus_ReturnsFalse()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);
        reservation.Cancel();

        // Act & Assert
        Assert.False(reservation.IsValid);
    }

    [Fact]
    public void ReservationEntity_Hydrate_ReconstructsEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddMinutes(-5);
        var expiresAt = DateTime.UtcNow.AddMinutes(10);
        var completedAt = DateTime.UtcNow;

        // Act
        var reservation = ReservationEntity.Hydrate(
            id, 1, 5, "Completed", createdAt, expiresAt, completedAt);

        // Assert
        Assert.Equal(id, reservation.Id.Value);
        Assert.Equal(1, reservation.ProductId);
        Assert.Equal(5, reservation.Quantity);
        Assert.Equal(ReservationStatus.Completed, reservation.Status);
        Assert.Equal(createdAt, reservation.CreatedAt);
        Assert.Equal(expiresAt, reservation.ExpiresAt);
        Assert.Equal(completedAt, reservation.CompletedAt);
    }

    #endregion

    #region UserEntity Tests

    [Fact]
    public void UserEntity_Create_WithValidData_CreatesEntity()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");

        // Act
        var user = UserEntity.Create("testuser", passwordHash);

        // Assert
        Assert.Equal("testuser", user.Username);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Equal(0, user.Id); // ID is 0 until persisted
    }

    [Fact]
    public void UserEntity_Create_WithEmptyUsername_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => UserEntity.Create("", "hash"));
        Assert.Contains("Username", exception.Message);
    }

    [Fact]
    public void UserEntity_Create_WithWhitespaceUsername_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => UserEntity.Create("   ", "hash"));
        Assert.Contains("Username", exception.Message);
    }

    [Fact]
    public void UserEntity_Create_WithShortUsername_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => UserEntity.Create("ab", "hash"));
        Assert.Contains("3 and 50", exception.Message);
    }

    [Fact]
    public void UserEntity_Create_WithLongUsername_ThrowsArgumentException()
    {
        // Arrange
        var longUsername = new string('a', 51);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => UserEntity.Create(longUsername, "hash"));
        Assert.Contains("3 and 50", exception.Message);
    }

    [Fact]
    public void UserEntity_Create_WithMinLengthUsername_Succeeds()
    {
        // Act
        var user = UserEntity.Create("abc", "hash");

        // Assert
        Assert.Equal("abc", user.Username);
    }

    [Fact]
    public void UserEntity_Create_WithMaxLengthUsername_Succeeds()
    {
        // Arrange
        var maxUsername = new string('a', 50);

        // Act
        var user = UserEntity.Create(maxUsername, "hash");

        // Assert
        Assert.Equal(maxUsername, user.Username);
    }

    [Fact]
    public void UserEntity_VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "testpassword123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = UserEntity.Create("testuser", passwordHash);

        // Act
        var result = user.VerifyPassword(password);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UserEntity_VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = UserEntity.Create("testuser", passwordHash);

        // Act
        var result = user.VerifyPassword("wrongpassword");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UserEntity_UpdatePassword_UpdatesPasswordHash()
    {
        // Arrange
        var originalHash = BCrypt.Net.BCrypt.HashPassword("original");
        var user = UserEntity.Create("testuser", originalHash);
        var newHash = BCrypt.Net.BCrypt.HashPassword("newpassword");

        // Act
        user.UpdatePassword(newHash);

        // Assert
        Assert.True(user.VerifyPassword("newpassword"));
        Assert.False(user.VerifyPassword("original"));
    }

    [Fact]
    public void UserEntity_Hydrate_ReconstructsEntity()
    {
        // Act
        var user = UserEntity.Hydrate(1, "testuser", "somehash");

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("somehash", user.PasswordHash);
    }

    [Fact]
    public void UserEntity_Hydrate_WithNullValues_UsesEmptyStrings()
    {
        // Act
        var user = UserEntity.Hydrate(1, null!, null!);

        // Assert
        Assert.Equal(1, user.Id);
        Assert.Equal(string.Empty, user.Username);
        Assert.Equal(string.Empty, user.PasswordHash);
    }

    #endregion

    #region ReservationAuditEntity Tests

    [Fact]
    public void ReservationAuditEntity_Create_WithReservation_CreatesAuditRecord()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);

        // Act
        var audit = ReservationAuditEntity.Create(reservation, "Test audit");

        // Assert
        Assert.Equal(reservation.Id, audit.ReservationId);
        Assert.Equal(reservation.ProductId, audit.ProductId);
        Assert.Equal(reservation.Quantity, audit.Quantity);
        Assert.Equal(reservation.Status, audit.Status);
        Assert.Equal("Test audit", audit.Notes);
    }

    [Fact]
    public void ReservationAuditEntity_Create_WithNullNotes_SetsNullNotes()
    {
        // Arrange
        var reservation = ReservationEntity.Create(1, 5, 15);

        // Act
        var audit = ReservationAuditEntity.Create(reservation, null);

        // Assert
        Assert.Null(audit.Notes);
    }

    [Fact]
    public void ReservationAuditEntity_Hydrate_ReconstructsEntity()
    {
        // Arrange
        var id = 1;
        var reservationId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        // Act
        var audit = ReservationAuditEntity.Hydrate(
            id, reservationId, 1, 5, "Reserved", timestamp, "Test notes");

        // Assert
        Assert.Equal(id, audit.Id);
        Assert.Equal(reservationId, audit.ReservationId.Value);
        Assert.Equal(1, audit.ProductId);
        Assert.Equal(5, audit.Quantity);
        Assert.Equal(ReservationStatus.Reserved, audit.Status);
        Assert.Equal(timestamp, audit.Timestamp);
        Assert.Equal("Test notes", audit.Notes);
    }

    #endregion
}
