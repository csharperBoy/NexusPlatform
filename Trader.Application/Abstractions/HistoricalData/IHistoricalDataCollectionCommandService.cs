using Trader.Application.Dtos.HistoricalData;

namespace Trader.Application.Abstractions.HistoricalData;

public interface IHistoricalDataCollectionCommandService
{
    /// <summary>ساخت Job در حالت Backfill (State=Created).</summary>
    Task<Guid> CreateBackfillAsync(
        CreateHistoricalBackfillModel model, CancellationToken ct = default);

    /// <summary>ساخت Job در حالت Recurring (State=Created).</summary>
    Task<Guid> CreateRecurringAsync(
        CreateHistoricalRecurringModel model, CancellationToken ct = default);

    /// <summary>
    /// شروع Job. Backfill: ScheduleAsync فوری. Recurring: ScheduleRecurringAsync +1min.
    /// Idempotent اگر State=Running.
    /// </summary>
    Task StartJobAsync(Guid id, CancellationToken ct = default);

    /// <summary>توقف Job در Running (no-op اگر غیر Running). Scheduler را Cancel می‌کند.</summary>
    Task StopJobAsync(Guid id, CancellationToken ct = default);

    Task SaveAsync(CancellationToken ct = default);
}