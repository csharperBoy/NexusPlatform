using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// رفع ابهام SL/TP در یک کندل با استفاده از Sub-Candleها.
/// اگر sub-candles موجود نباشد → Pessimistic (SL اول).
/// </summary>
public interface ISubCandleResolver
{
    SLTPResolution Resolve(
        decimal? stopLoss,
        decimal? takeProfit,
        Candle primaryCandle,
        IReadOnlyList<Candle>? subCandles);
}

public sealed record SLTPResolution(
    bool Triggered,
    decimal Price,
    BacktestExitReason Reason)
{
    public static SLTPResolution None =>
        new(false, 0m, BacktestExitReason.EndOfBacktest);
}