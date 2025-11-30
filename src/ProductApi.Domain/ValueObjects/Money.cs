namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Value object representing a monetary amount.
/// Ensures price is always positive and provides value equality.
/// </summary>
public sealed class Money : IEquatable<Money>
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

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => Amount.GetHashCode();

    public static bool operator ==(Money? left, Money? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public static implicit operator decimal(Money money) => money.Amount;

    public override string ToString() => Amount.ToString("C");
}
