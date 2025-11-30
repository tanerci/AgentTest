namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Value object representing a unique reservation identifier.
/// </summary>
public sealed class ReservationId : ValueObject<ReservationId>
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

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
