namespace Trader.Application.Abstractions.HistoricalData;

public interface IHistoricalDataCollectionCommandService
{
    /// <summary>ساخت یک Collection Job جدید. Id را برمی‌گرداند.</summary>
    Task<Guid> CreateAsync(
        string name,
        List<string> isins,
        List<int> timeFrames,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken ct = default);

    /// <summary>لغو یک Job (Pending/Running → Cancelled).</summary>
    Task<bool> CancelAsync(Guid id, CancellationToken ct = default);

    Task SaveAsync(CancellationToken ct = default);
}