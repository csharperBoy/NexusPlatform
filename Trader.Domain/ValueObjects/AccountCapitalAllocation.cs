using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.ValueObjects;

/// <summary>
/// تخصیص سرمایه برای یک (Run, Account) — Owned Type.
///
/// دو حالت:
///   • Fixed: مقدار ثابت (مثلاً 50M ریال)
///   • Dynamic: درصدی از BuyPower موجود در لحظه‌ی Order (0, 100]
///
/// Invariants:
///   • Fixed: FixedAmount.HasValue && PercentOfBuyPower == null
///   • Dynamic: PercentOfBuyPower.HasValue && FixedAmount == null
///
/// Placeholder برای Run-level CapitalAllocation قبلی — همان‌جا در VO مانده،
/// ولی حالا per-account استفاده می‌شود.
/// </summary>
public sealed class AccountCapitalAllocation : ValueObject
{
    public CapitalAllocationMode Mode { get; private set; }
    public Money? FixedAmount { get; private set; }
    public decimal? PercentOfBuyPower { get; private set; }

    private AccountCapitalAllocation() { } // EF

    public static AccountCapitalAllocation Fixed(Money amount)
    {
        if (amount is null)
            throw new ArgumentNullException(nameof(amount));
        if (amount.Amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount),
                "Fixed capital amount must be greater than zero.");

        return new AccountCapitalAllocation
        {
            Mode = CapitalAllocationMode.Fixed,
            FixedAmount = amount,
            PercentOfBuyPower = null,
        };
    }

    public static AccountCapitalAllocation Dynamic(decimal percentOfBuyPower)
    {
        if (percentOfBuyPower <= 0m || percentOfBuyPower > 100m)
            throw new ArgumentOutOfRangeException(nameof(percentOfBuyPower),
                "PercentOfBuyPower must be in (0, 100].");

        return new AccountCapitalAllocation
        {
            Mode = CapitalAllocationMode.Dynamic,
            FixedAmount = null,
            PercentOfBuyPower = percentOfBuyPower,
        };
    }

    public bool IsFixed => Mode == CapitalAllocationMode.Fixed;
    public bool IsDynamic => Mode == CapitalAllocationMode.Dynamic;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Mode;
        yield return FixedAmount;
        yield return PercentOfBuyPower;
    }
}