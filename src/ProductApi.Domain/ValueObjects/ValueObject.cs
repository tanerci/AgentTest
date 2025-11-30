namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Base class for value objects that provides common equality semantics.
/// Value objects are immutable and have equality based on their component values.
/// </summary>
/// <typeparam name="T">The derived value object type.</typeparam>
public abstract class ValueObject<T> : IEquatable<T> where T : ValueObject<T>
{
    /// <summary>
    /// Gets the components used to determine equality.
    /// </summary>
    /// <returns>An enumerable of component values.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(T? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override bool Equals(object? obj) => Equals(obj as T);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var component in GetEqualityComponents())
        {
            hashCode.Add(component);
        }
        return hashCode.ToHashCode();
    }

    public static bool operator ==(ValueObject<T>? left, ValueObject<T>? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject<T>? left, ValueObject<T>? right) => !(left == right);
}
