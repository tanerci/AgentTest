namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Value object representing a product name.
/// Ensures name is not empty and within length constraints.
/// </summary>
public sealed class ProductName : ValueObject<ProductName>
{
    public const int MaxLength = 100;
    public const int MinLength = 1;

    public string Value { get; }

    private ProductName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a ProductName instance from a string.
    /// </summary>
    /// <param name="name">The product name (1-100 characters).</param>
    /// <returns>A ProductName value object.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null, empty, or exceeds limits.</exception>
    public static ProductName Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (name.Length < MinLength || name.Length > MaxLength)
            throw new ArgumentException($"Product name must be between {MinLength} and {MaxLength} characters", nameof(name));

        return new ProductName(name.Trim());
    }

    /// <summary>
    /// Creates a ProductName instance from a string, returning null if invalid.
    /// </summary>
    public static ProductName? TryCreate(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        if (name.Length < MinLength || name.Length > MaxLength) return null;
        return new ProductName(name.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public static implicit operator string(ProductName name) => name.Value;

    public override string ToString() => Value;
}
