namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Value object representing product stock quantity.
/// Ensures stock is never negative.
/// </summary>
public sealed class Stock : ValueObject<Stock>
{
    public int Quantity { get; }

    private Stock(int quantity)
    {
        Quantity = quantity;
    }

    /// <summary>
    /// Creates a Stock instance from an integer quantity.
    /// </summary>
    /// <param name="quantity">The stock quantity (must be non-negative).</param>
    /// <returns>A Stock value object.</returns>
    /// <exception cref="ArgumentException">Thrown when quantity is negative.</exception>
    public static Stock Create(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

        return new Stock(quantity);
    }

    /// <summary>
    /// Creates a Stock instance from an integer, returning null if negative.
    /// </summary>
    public static Stock? TryCreate(int quantity)
    {
        return quantity >= 0 ? new Stock(quantity) : null;
    }

    /// <summary>
    /// Adds quantity to the current stock.
    /// </summary>
    public Stock Add(int quantity)
    {
        return Create(Quantity + quantity);
    }

    /// <summary>
    /// Removes quantity from the current stock.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when resulting quantity would be negative.</exception>
    public Stock Remove(int quantity)
    {
        if (Quantity < quantity)
            throw new InvalidOperationException($"Cannot remove {quantity} items from stock of {Quantity}");

        return Create(Quantity - quantity);
    }

    /// <summary>
    /// Checks if there is sufficient stock.
    /// </summary>
    public bool HasSufficientStock(int required) => Quantity >= required;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Quantity;
    }

    public static implicit operator int(Stock stock) => stock.Quantity;

    public override string ToString() => Quantity.ToString();
}
