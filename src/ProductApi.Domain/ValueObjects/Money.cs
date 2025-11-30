namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount.
/// Ensures price is always positive and provides value equality.
/// </summary>
public sealed class Money : ValueObject<Money>
{
    public decimal Amount { get; }

    private Money(decimal amount)
    {
        Amount = amount;
    }

    /// <summary>
    /// Creates a Money instance from a decimal amount.
    /// </summary>
    /// <param name="amount">The monetary amount (must be non-negative).</param>
    /// <returns>A Money value object.</returns>
    /// <exception cref="ArgumentException">Thrown when amount is negative.</exception>
    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative", nameof(amount));

        return new Money(amount);
    }

    /// <summary>
    /// Creates a Money instance from a decimal, returning null if amount is negative.
    /// </summary>
    public static Money? TryCreate(decimal amount)
    {
        return amount >= 0 ? new Money(amount) : null;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
    }

    public static implicit operator decimal(Money money) => money.Amount;

    public override string ToString() => Amount.ToString("C");
}
