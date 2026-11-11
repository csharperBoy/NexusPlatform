namespace Trader.Application.Abstractions.HistoricalData;

public interface IHistoricalDataCollector
{
    /// <summary>Backfill — از DateFrom تا DateTo، سپس MarkCompleted.</summary>
    Task CollectBackfillAsync(Guid jobId, CancellationToken ct = default);

    /// <summary>Recurring — یک tick: از boundary هر (Isin, TF) تا now، بدون تغییر State.</summary>
    Task CollectRecurringAsync(Guid jobId, CancellationToken ct = default);
}