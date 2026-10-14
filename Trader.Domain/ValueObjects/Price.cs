namespace Trader.Domain.ValueObjects;

public readonly record struct Price : IComparable<Price>
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        if (value < 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Price cannot be negative.");
        Value = value;
    }

    public static Price Zero => new(0m);

    public int CompareTo(Price other) => Value.CompareTo(other.Value);

    public static bool operator >(Price a, Price b) => a.Value > b.Value;
    public static bool operator <(Price a, Price b) => a.Value < b.Value;
    public static bool operator >=(Price a, Price b) => a.Value >= b.Value;
    public static bool operator <=(Price a, Price b) => a.Value <= b.Value;

    public override string ToString() => Value.ToString("0.####");
}