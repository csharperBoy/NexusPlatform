using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// اتصال یک TradingAccount به یک TradingRun با درصد تخصیص از BuyPower.
///
/// AllocationPercent معنایش: «حداکثر این درصد از BuyPower موجود در لحظه‌ی Order».
/// این درصد رزرو نمی‌شود. اگر Run دیگری از همین Account استفاده کند،
/// BuyPower موجود در لحظه‌ی Order دیده می‌شود.
///
/// مثال: Account با BuyPower = 200M.
///   R1 (50%) + R2 (100%) — جمع = 150%.
///   R1 اول 60M خرید → BuyPower = 140M.
///   R2 (100%) → می‌تواند تا 140M خرید کند.
/// </summary>
public class TradingRunAccount : BaseEntity
{
    public Guid TradingRunId { get; private set; }
    public Guid TraderAccountId { get; private set; }

    /// <summary>درصد از BuyPower موجود در لحظه‌ی Order. Range: (0, 100].</summary>
    public decimal AllocationPercent { get; private set; }

    private TradingRunAccount() { } // EF

    internal static TradingRunAccount Create(
        Guid runId,
        Guid accountId,
        decimal allocationPercent = 100m)
    {
        if (allocationPercent <= 0m || allocationPercent > 100m)
            throw new ArgumentOutOfRangeException(nameof(allocationPercent),
                "AllocationPercent must be in (0, 100].");

        return new TradingRunAccount
        {
            TradingRunId = runId,
            TraderAccountId = accountId,
            AllocationPercent = allocationPercent,
        };
    }

    internal void UpdateAllocation(decimal allocationPercent)
    {
        if (allocationPercent <= 0m || allocationPercent > 100m)
            throw new ArgumentOutOfRangeException(nameof(allocationPercent),
                "AllocationPercent must be in (0, 100].");

        AllocationPercent = allocationPercent;
    }
}