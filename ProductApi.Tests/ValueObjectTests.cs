using ProductApi.Domain.ValueObjects;
using Xunit;

namespace ProductApi.Tests;

/// <summary>
/// Tests for domain value objects to ensure business rules and value equality.
/// </summary>
public class ValueObjectTests
{
    #region Money Tests

    [Fact]
    public void Money_Create_WithPositiveAmount_Succeeds()
    {
        // Act
        var money = Money.Create(99.99m);

        // Assert
        Assert.Equal(99.99m, money.Amount);
    }

    [Fact]
    public void Money_Create_WithZeroAmount_Succeeds()
    {
        // Act
        var money = Money.Create(0m);

        // Assert
        Assert.Equal(0m, money.Amount);
    }

    [Fact]
    public void Money_Create_WithNegativeAmount_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Money.Create(-10m));
        Assert.Contains("negative", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Money_TryCreate_WithValidAmount_ReturnsMoney()
    {
        // Act
        var money = Money.TryCreate(50m);

        // Assert
        Assert.NotNull(money);
        Assert.Equal(50m, money.Amount);
    }

    [Fact]
    public void Money_TryCreate_WithNegativeAmount_ReturnsNull()
    {
        // Act
        var money = Money.TryCreate(-10m);

        // Assert
        Assert.Null(money);
    }

    [Fact]
    public void Money_Equality_SameAmount_ReturnsTrue()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(100m);

        // Act & Assert
        Assert.Equal(money1, money2);
        Assert.True(money1 == money2);
        Assert.False(money1 != money2);
    }

    [Fact]
    public void Money_Equality_DifferentAmounts_ReturnsFalse()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(200m);

        // Act & Assert
        Assert.NotEqual(money1, money2);
        Assert.False(money1 == money2);
        Assert.True(money1 != money2);
    }

    [Fact]
    public void Money_ImplicitConversionToDecimal_ReturnsAmount()
    {
        // Arrange
        var money = Money.Create(75.50m);

        // Act
        decimal amount = money;

        // Assert
        Assert.Equal(75.50m, amount);
    }

    [Fact]
    public void Money_Equality_WithNull_ReturnsFalse()
    {
        // Arrange
        var money = Money.Create(100m);

        // Act & Assert
        Assert.False(money.Equals(null));
        Assert.False(money == null);
        Assert.True(money != null);
    }

    [Fact]
    public void Money_GetHashCode_SameValue_ReturnsSameHash()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(100m);

        // Act & Assert
        Assert.Equal(money1.GetHashCode(), money2.GetHashCode());
    }

    #endregion

    #region Stock Tests

    [Fact]
    public void Stock_Create_WithPositiveQuantity_Succeeds()
    {
        // Act
        var stock = Stock.Create(50);

        // Assert
        Assert.Equal(50, stock.Quantity);
    }

    [Fact]
    public void Stock_Create_WithZeroQuantity_Succeeds()
    {
        // Act
        var stock = Stock.Create(0);

        // Assert
        Assert.Equal(0, stock.Quantity);
    }

    [Fact]
    public void Stock_Create_WithNegativeQuantity_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Stock.Create(-5));
        Assert.Contains("negative", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Stock_TryCreate_WithValidQuantity_ReturnsStock()
    {
        // Act
        var stock = Stock.TryCreate(25);

        // Assert
        Assert.NotNull(stock);
        Assert.Equal(25, stock.Quantity);
    }

    [Fact]
    public void Stock_TryCreate_WithNegativeQuantity_ReturnsNull()
    {
        // Act
        var stock = Stock.TryCreate(-5);

        // Assert
        Assert.Null(stock);
    }

    [Fact]
    public void Stock_Add_IncreasesQuantity()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var newStock = stock.Add(5);

        // Assert
        Assert.Equal(15, newStock.Quantity);
    }

    [Fact]
    public void Stock_Remove_DecreasesQuantity()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act
        var newStock = stock.Remove(3);

        // Assert
        Assert.Equal(7, newStock.Quantity);
    }

    [Fact]
    public void Stock_Remove_WithInsufficientStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var stock = Stock.Create(5);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => stock.Remove(10));
        Assert.Contains("Cannot remove", exception.Message);
    }

    [Fact]
    public void Stock_HasSufficientStock_WithEnoughStock_ReturnsTrue()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act & Assert
        Assert.True(stock.HasSufficientStock(5));
        Assert.True(stock.HasSufficientStock(10));
    }

    [Fact]
    public void Stock_HasSufficientStock_WithInsufficientStock_ReturnsFalse()
    {
        // Arrange
        var stock = Stock.Create(5);

        // Act & Assert
        Assert.False(stock.HasSufficientStock(10));
    }

    [Fact]
    public void Stock_Equality_SameQuantity_ReturnsTrue()
    {
        // Arrange
        var stock1 = Stock.Create(50);
        var stock2 = Stock.Create(50);

        // Act & Assert
        Assert.Equal(stock1, stock2);
        Assert.True(stock1 == stock2);
    }

    [Fact]
    public void Stock_Equality_DifferentQuantity_ReturnsFalse()
    {
        // Arrange
        var stock1 = Stock.Create(50);
        var stock2 = Stock.Create(25);

        // Act & Assert
        Assert.NotEqual(stock1, stock2);
        Assert.False(stock1 == stock2);
    }

    [Fact]
    public void Stock_ImplicitConversionToInt_ReturnsQuantity()
    {
        // Arrange
        var stock = Stock.Create(75);

        // Act
        int quantity = stock;

        // Assert
        Assert.Equal(75, quantity);
    }

    [Fact]
    public void Stock_Equality_WithNull_ReturnsFalse()
    {
        // Arrange
        var stock = Stock.Create(10);

        // Act & Assert
        Assert.False(stock.Equals(null));
    }

    #endregion

    #region ProductName Tests

    [Fact]
    public void ProductName_Create_WithValidName_Succeeds()
    {
        // Act
        var productName = ProductName.Create("Test Product");

        // Assert
        Assert.Equal("Test Product", productName.Value);
    }

    [Fact]
    public void ProductName_Create_TrimsWhitespace()
    {
        // Act
        var productName = ProductName.Create("  Test Product  ");

        // Assert
        Assert.Equal("Test Product", productName.Value);
    }

    [Fact]
    public void ProductName_Create_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ProductName.Create(""));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductName_Create_WithWhitespaceOnly_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ProductName.Create("   "));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProductName_Create_WithTooLongName_ThrowsArgumentException()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ProductName.Create(longName));
        Assert.Contains("100", exception.Message);
    }

    [Fact]
    public void ProductName_Create_WithMaxLengthName_Succeeds()
    {
        // Arrange
        var maxLengthName = new string('a', 100);

        // Act
        var productName = ProductName.Create(maxLengthName);

        // Assert
        Assert.Equal(100, productName.Value.Length);
    }

    [Fact]
    public void ProductName_TryCreate_WithValidName_ReturnsProductName()
    {
        // Act
        var productName = ProductName.TryCreate("Valid Name");

        // Assert
        Assert.NotNull(productName);
        Assert.Equal("Valid Name", productName.Value);
    }

    [Fact]
    public void ProductName_TryCreate_WithNull_ReturnsNull()
    {
        // Act
        var productName = ProductName.TryCreate(null);

        // Assert
        Assert.Null(productName);
    }

    [Fact]
    public void ProductName_TryCreate_WithEmptyString_ReturnsNull()
    {
        // Act
        var productName = ProductName.TryCreate("");

        // Assert
        Assert.Null(productName);
    }

    [Fact]
    public void ProductName_TryCreate_WithTooLongName_ReturnsNull()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        var productName = ProductName.TryCreate(longName);

        // Assert
        Assert.Null(productName);
    }

    [Fact]
    public void ProductName_Equality_SameName_ReturnsTrue()
    {
        // Arrange
        var name1 = ProductName.Create("Product A");
        var name2 = ProductName.Create("Product A");

        // Act & Assert
        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
    }

    [Fact]
    public void ProductName_Equality_DifferentCase_ReturnsTrue()
    {
        // Arrange
        var name1 = ProductName.Create("Product A");
        var name2 = ProductName.Create("PRODUCT A");

        // Act & Assert
        Assert.Equal(name1, name2);
        Assert.True(name1 == name2);
    }

    [Fact]
    public void ProductName_Equality_DifferentNames_ReturnsFalse()
    {
        // Arrange
        var name1 = ProductName.Create("Product A");
        var name2 = ProductName.Create("Product B");

        // Act & Assert
        Assert.NotEqual(name1, name2);
        Assert.False(name1 == name2);
    }

    [Fact]
    public void ProductName_ImplicitConversionToString_ReturnsValue()
    {
        // Arrange
        var productName = ProductName.Create("Test Product");

        // Act
        string name = productName;

        // Assert
        Assert.Equal("Test Product", name);
    }

    [Fact]
    public void ProductName_Equality_WithNull_ReturnsFalse()
    {
        // Arrange
        var productName = ProductName.Create("Test");

        // Act & Assert
        Assert.False(productName.Equals(null));
    }

    #endregion

    #region ReservationId Tests

    [Fact]
    public void ReservationId_Create_GeneratesUniqueId()
    {
        // Act
        var id1 = ReservationId.Create();
        var id2 = ReservationId.Create();

        // Assert
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void ReservationId_FromGuid_WithValidGuid_Succeeds()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var reservationId = ReservationId.FromGuid(guid);

        // Assert
        Assert.Equal(guid, reservationId.Value);
    }

    [Fact]
    public void ReservationId_FromGuid_WithEmptyGuid_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationId.FromGuid(Guid.Empty));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReservationId_TryParse_WithValidGuidString_ReturnsReservationId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var reservationId = ReservationId.TryParse(guid.ToString());

        // Assert
        Assert.NotNull(reservationId);
        Assert.Equal(guid, reservationId.Value);
    }

    [Fact]
    public void ReservationId_TryParse_WithInvalidString_ReturnsNull()
    {
        // Act
        var reservationId = ReservationId.TryParse("not-a-guid");

        // Assert
        Assert.Null(reservationId);
    }

    [Fact]
    public void ReservationId_TryParse_WithEmptyGuidString_ReturnsNull()
    {
        // Act
        var reservationId = ReservationId.TryParse(Guid.Empty.ToString());

        // Assert
        Assert.Null(reservationId);
    }

    [Fact]
    public void ReservationId_Equality_SameGuid_ReturnsTrue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ReservationId.FromGuid(guid);
        var id2 = ReservationId.FromGuid(guid);

        // Act & Assert
        Assert.Equal(id1, id2);
        Assert.True(id1 == id2);
    }

    [Fact]
    public void ReservationId_Equality_DifferentGuids_ReturnsFalse()
    {
        // Arrange
        var id1 = ReservationId.Create();
        var id2 = ReservationId.Create();

        // Act & Assert
        Assert.NotEqual(id1, id2);
        Assert.False(id1 == id2);
    }

    [Fact]
    public void ReservationId_ToString_ReturnsGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var reservationId = ReservationId.FromGuid(guid);

        // Act
        var result = reservationId.ToString();

        // Assert
        Assert.Equal(guid.ToString(), result);
    }

    [Fact]
    public void ReservationId_Equality_WithNull_ReturnsFalse()
    {
        // Arrange
        var id = ReservationId.Create();

        // Act & Assert
        Assert.False(id.Equals(null));
    }

    #endregion

    #region ReservationStatus Tests

    [Fact]
    public void ReservationStatus_Reserved_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Reserved", ReservationStatus.Reserved.Value);
    }

    [Fact]
    public void ReservationStatus_Expired_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Expired", ReservationStatus.Expired.Value);
    }

    [Fact]
    public void ReservationStatus_Completed_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Completed", ReservationStatus.Completed.Value);
    }

    [Fact]
    public void ReservationStatus_Cancelled_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Cancelled", ReservationStatus.Cancelled.Value);
    }

    [Fact]
    public void ReservationStatus_FromString_WithValidStatus_ReturnsStatus()
    {
        // Act
        var status = ReservationStatus.FromString("Reserved");

        // Assert
        Assert.Equal("Reserved", status.Value);
    }

    [Fact]
    public void ReservationStatus_FromString_IsCaseInsensitive()
    {
        // Act - FromString preserves original case but equality is case-insensitive
        var status1 = ReservationStatus.FromString("reserved");
        var status2 = ReservationStatus.FromString("RESERVED");

        // Assert - Values preserve original case
        Assert.Equal("reserved", status1.Value);
        Assert.Equal("RESERVED", status2.Value);
        // But equality comparison is case-insensitive
        Assert.True(status1 == status2);
        Assert.Equal(status1, status2);
    }

    [Fact]
    public void ReservationStatus_FromString_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationStatus.FromString(""));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReservationStatus_FromString_WithInvalidStatus_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ReservationStatus.FromString("InvalidStatus"));
        Assert.Contains("Invalid reservation status", exception.Message);
    }

    [Fact]
    public void ReservationStatus_CanCheckout_OnlyReservedReturnsTrue()
    {
        // Assert
        Assert.True(ReservationStatus.Reserved.CanCheckout);
        Assert.False(ReservationStatus.Expired.CanCheckout);
        Assert.False(ReservationStatus.Completed.CanCheckout);
        Assert.False(ReservationStatus.Cancelled.CanCheckout);
    }

    [Fact]
    public void ReservationStatus_CanCancel_OnlyReservedReturnsTrue()
    {
        // Assert
        Assert.True(ReservationStatus.Reserved.CanCancel);
        Assert.False(ReservationStatus.Expired.CanCancel);
        Assert.False(ReservationStatus.Completed.CanCancel);
        Assert.False(ReservationStatus.Cancelled.CanCancel);
    }

    [Fact]
    public void ReservationStatus_IsActive_OnlyReservedReturnsTrue()
    {
        // Assert
        Assert.True(ReservationStatus.Reserved.IsActive);
        Assert.False(ReservationStatus.Expired.IsActive);
        Assert.False(ReservationStatus.Completed.IsActive);
        Assert.False(ReservationStatus.Cancelled.IsActive);
    }

    [Fact]
    public void ReservationStatus_Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var status1 = ReservationStatus.Reserved;
        var status2 = ReservationStatus.Reserved;

        // Act & Assert
        Assert.Equal(status1, status2);
        Assert.True(status1 == status2);
    }

    [Fact]
    public void ReservationStatus_Equality_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var status1 = ReservationStatus.Reserved;
        var status2 = ReservationStatus.Completed;

        // Act & Assert
        Assert.NotEqual(status1, status2);
        Assert.False(status1 == status2);
    }

    [Fact]
    public void ReservationStatus_Equality_WithNull_ReturnsFalse()
    {
        // Arrange
        var status = ReservationStatus.Reserved;

        // Act & Assert
        Assert.False(status.Equals(null));
    }

    [Fact]
    public void ReservationStatus_ToString_ReturnsValue()
    {
        // Assert
        Assert.Equal("Reserved", ReservationStatus.Reserved.ToString());
        Assert.Equal("Completed", ReservationStatus.Completed.ToString());
    }

    #endregion
}
