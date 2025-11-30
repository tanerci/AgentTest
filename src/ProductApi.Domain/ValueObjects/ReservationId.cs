namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique reservation identifier.
/// </summary>
public sealed class ReservationId : IEquatable<ReservationId>
{
    public Guid Value { get; }

    private ReservationId(Guid value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new unique ReservationId.
    /// </summary>
    public static ReservationId Create()
    {
        return new ReservationId(Guid.NewGuid());
    }

    /// <summary>
    /// Creates a ReservationId from an existing Guid.
    /// </summary>
    public static ReservationId FromGuid(Guid guid)
    {
        if (guid == Guid.Empty)
            throw new ArgumentException("Reservation ID cannot be empty", nameof(guid));

        return new ReservationId(guid);
    }

    /// <summary>
    /// Attempts to parse a string into a ReservationId.
    /// </summary>
    public static ReservationId? TryParse(string value)
    {
        if (Guid.TryParse(value, out var guid) && guid != Guid.Empty)
            return new ReservationId(guid);

        return null;
    }

    public bool Equals(ReservationId? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as ReservationId);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(ReservationId? left, ReservationId? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ReservationId? left, ReservationId? right) => !(left == right);

    public override string ToString() => Value.ToString();
}
