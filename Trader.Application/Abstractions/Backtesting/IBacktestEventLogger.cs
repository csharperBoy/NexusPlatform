using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// Buffer می‌کند رویدادها را در حافظه، فقط با FlushAsync به DB می‌نویسد.
/// Scoped — یک نمونه در هر Backtest scope.
/// </summary>
public interface IBacktestEventLogger
{
    void Log(
        Guid backtestRunId,
        BacktestEventType type,
        BacktestEventSeverity severity,
        string message,
        object? payload = null,
        string? isin = null,
        DateTime? candleTime = null,
        Guid? tradeId = null);

    Task FlushAsync(CancellationToken ct = default);
}