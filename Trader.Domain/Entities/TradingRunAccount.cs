using Core.Domain.Common.EntityProperties;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// اتصال یک TraderAccount به یک TradingRun.
///
/// CapitalAllocation حالا per-account است (Phase D):
///   • Mode: Fixed یا Dynamic (مستقل برای هر Account)
///   • Fixed: مقدار ثابت (ریال)
///   • Dynamic: درصدی از BuyPower لحظه‌ی Order (0, 100]
///
/// یک Account می‌تواند در چند Run باشد — هر Run سهم خودش را از
/// BuyPower آن Account می‌بیند (بدون رزرو).
/// </summary>
public class TradingRunAccount : BaseEntity
{
    public Guid TradingRunId { get; private set; }
    public Guid TraderAccountId { get; private set; }
    public AccountCapitalAllocation CapitalAllocation { get; private set; } = default!;

    private TradingRunAccount() { } // EF

    internal static TradingRunAccount Create(
        Guid runId,
        Guid accountId,
        AccountCapitalAllocation allocation)
    {
        if (allocation is null)
            throw new ArgumentNullException(nameof(allocation));

        return new TradingRunAccount
        {
            TradingRunId = runId,
            TraderAccountId = accountId,
            CapitalAllocation = allocation,
        };
    }

    internal void UpdateCapitalAllocation(AccountCapitalAllocation allocation)
    {
        if (allocation is null)
            throw new ArgumentNullException(nameof(allocation));
        CapitalAllocation = allocation;
    }
}