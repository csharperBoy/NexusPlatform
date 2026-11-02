namespace Trader.Application.Abstractions.HistoricalData;

public interface IHistoricalDataCollector
{
    /// <summary>اجرای یک CollectionJob با Id مشخص.</summary>
    Task RunAsync(Guid jobId, CancellationToken ct = default);
}