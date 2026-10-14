namespace Trader.Domain.ValueObjects;

public readonly record struct Quantity
{
    public long Value { get; }

    public Quantity(long value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Quantity cannot be negative.");
        Value = value;
    }

    public static Quantity Zero => new(0);

    public bool IsZero => Value == 0;
    public bool IsPositive => Value > 0;

    public override string ToString() => Value.ToString("N0");
}