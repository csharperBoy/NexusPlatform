namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// Pure in-memory simulation — بدون DB write. برای Backtest یک‌باره و
/// برای tuning (هزاران اجرا) استفاده می‌شود.
///
/// Behavioral contract (مطابق D55-D58):
///   • Two-Stream (IRR + transformed برای Strategy)
///   • FillMath برای commission/PnL
///   • Sub-candle resolution برای SL/TP
///   • SignalCurrencyHelper.ScaleSlTpToIrr وقتی needsTransform
///   • Daily equity در TehranDate
///
/// پیاده‌سازی thread-safe است (برای tuning موازی). eventLogger می‌تواند
/// NullBacktestEventLogger.Instance باشد.
/// </summary>
public interface IBacktestSimulator
{
    Task<BacktestSimulationResult> SimulateAsync(
        BacktestSimulationRequest request,
        IBacktestEventLogger eventLogger,
        CancellationToken ct = default);
}