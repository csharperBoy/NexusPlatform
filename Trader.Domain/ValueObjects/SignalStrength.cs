namespace Trader.Domain.ValueObjects;

/// <summary>قدرت سیگنال در بازه [0,1].</summary>
public readonly record struct SignalStrength
{
    public decimal Value { get; }

    public SignalStrength(decimal value)
    {
        if (value < 0m || value > 1m)
            throw new ArgumentOutOfRangeException(nameof(value),
                "SignalStrength must be in [0,1].");
        Value = value;
    }

    public static SignalStrength Min => new(0m);
    public static SignalStrength Neutral => new(0.5m);
    public static SignalStrength Max => new(1m);

    public override string ToString() => Value.ToString("0.###");
}