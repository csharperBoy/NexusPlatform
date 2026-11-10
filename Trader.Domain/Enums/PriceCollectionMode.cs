namespace Trader.Domain.Enums;

public enum PriceCollectionMode
{
    /// <summary>Recurring — هر tick از Scheduler، یک‌بار collect (page 1 only).</summary>
    Recurring = 0,

    /// <summary>Backfill — یک‌بار کامل تا رسیدن به max(EffectiveAt) موجود در DB.</summary>
    Backfill = 1,
}