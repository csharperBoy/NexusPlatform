using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.ValueObjects;

/// <summary>تخصیص سرمایه برای یک TradingRun: Fixed یا Dynamic.</summary>
public sealed class CapitalAllocation : ValueObject
{
    public CapitalAllocationMode Mode { get; }
    public Money? FixedAmount { get; }

    private CapitalAllocation(CapitalAllocationMode mode, Money? fixedAmount)
    {
        Mode = mode;
        FixedAmount = fixedAmount;
    }

    public static CapitalAllocation Fixed(Money amount)
        => new(CapitalAllocationMode.Fixed, amount);

    public static CapitalAllocation Dynamic()
        => new(CapitalAllocationMode.Dynamic, null);

    public bool IsFixed => Mode == CapitalAllocationMode.Fixed;
    public bool IsDynamic => Mode == CapitalAllocationMode.Dynamic;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Mode;
        yield return FixedAmount;
    }
}