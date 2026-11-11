namespace Trader.Domain.Enums;

public enum HistoricalCollectionMode
{
    /// <summary>Backfill — یک‌بار اجرا: DateFrom تا DateTo، سپس Completed.</summary>
    Backfill = 0,

    /// <summary>Recurring — Scheduler: از max(OpenTime) هر (Isin, TF) تا now.</summary>
    Recurring = 1,
}