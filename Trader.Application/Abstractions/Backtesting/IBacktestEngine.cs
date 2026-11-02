namespace Trader.Application.Abstractions.Backtesting;

public interface IBacktestEngine
{
    /// <summary>
    /// اجرای Backtest با Id مشخص. BacktestRun باید در وضعیت Pending باشد.
    /// </summary>
    Task RunAsync(Guid backtestRunId, CancellationToken ct = default);
}