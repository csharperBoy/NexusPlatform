namespace Trader.Domain.ValueObjects;

/// <summary>درصد سرمایه‌ی یک Run که به یک نماد خاص تخصیص داده می‌شود.</summary>
public readonly record struct SymbolAllocation
{
    public decimal Percentage { get; }

    public SymbolAllocation(decimal percentage)
    {
        if (percentage < 0m || percentage > 100m)
            throw new ArgumentOutOfRangeException(nameof(percentage),
                "Percentage must be in [0,100].");
        Percentage = percentage;
    }

    public static SymbolAllocation Zero => new(0m);

    public Money ApplyTo(Money baseAmount)
        => new(baseAmount.Amount * Percentage / 100m, baseAmount.Currency);

    public override string ToString() => $"{Percentage:0.##}%";
}