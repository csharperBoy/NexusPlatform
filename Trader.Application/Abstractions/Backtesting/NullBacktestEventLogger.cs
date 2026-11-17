using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// پیاده‌سازی no-op برای tuning — بدون buffering، بدون flush.
/// Singleton، thread-safe.
/// </summary>
public sealed class NullBacktestEventLogger : IBacktestEventLogger
{
    public static readonly NullBacktestEventLogger Instance = new();

    private NullBacktestEventLogger() { }

    public void Log(
        Guid backtestRunId, BacktestEventType type, BacktestEventSeverity severity,
        string message, object? payload = null, string? isin = null,
        DateTime? candleTime = null, Guid? tradeId = null)
    {
        // no-op
    }

    public Task FlushAsync(CancellationToken ct = default) => Task.CompletedTask;
}