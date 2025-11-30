namespace ProductApi.Domain.ValueObjects;

/// <summary>
/// Value object representing the status of a reservation.
/// </summary>
public sealed class ReservationStatus : ValueObject<ReservationStatus>
{
    public string Value { get; }

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Reserved",
        "Expired",
        "Completed",
        "Cancelled"
    };

    private ReservationStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Status indicating the reservation is active.
    /// </summary>
    public static ReservationStatus Reserved => new("Reserved");

    /// <summary>
    /// Status indicating the reservation has expired due to TTL.
    /// </summary>
    public static ReservationStatus Expired => new("Expired");

    /// <summary>
    /// Status indicating the reservation has been checked out successfully.
    /// </summary>
    public static ReservationStatus Completed => new("Completed");

    /// <summary>
    /// Status indicating the reservation was cancelled.
    /// </summary>
    public static ReservationStatus Cancelled => new("Cancelled");

    /// <summary>
    /// Creates a ReservationStatus from a string value.
    /// </summary>
    public static ReservationStatus FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Status cannot be empty", nameof(value));

        if (!ValidStatuses.Contains(value))
            throw new ArgumentException($"Invalid reservation status: {value}", nameof(value));

        return new ReservationStatus(value);
    }

    /// <summary>
    /// Checks if the reservation can be checked out.
    /// </summary>
    public bool CanCheckout => Value == Reserved.Value;

    /// <summary>
    /// Checks if the reservation can be cancelled.
    /// </summary>
    public bool CanCancel => Value == Reserved.Value;

    /// <summary>
    /// Checks if the reservation is still active.
    /// </summary>
    public bool IsActive => Value == Reserved.Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
