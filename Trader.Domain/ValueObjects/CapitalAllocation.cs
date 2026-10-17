using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.ValueObjects;

public sealed class CapitalAllocation : ValueObject
{
    public CapitalAllocationMode Mode { get; private set; }
    public Money? FixedAmount { get; private set; }

    private CapitalAllocation() { } // EF

    public static CapitalAllocation Fixed(Money amount)
        => new() { Mode = CapitalAllocationMode.Fixed, FixedAmount = amount };

    public static CapitalAllocation Dynamic()
        => new() { Mode = CapitalAllocationMode.Dynamic, FixedAmount = null };

    public bool IsFixed => Mode == CapitalAllocationMode.Fixed;
    public bool IsDynamic => Mode == CapitalAllocationMode.Dynamic;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Mode;
        yield return FixedAmount;
    }
}