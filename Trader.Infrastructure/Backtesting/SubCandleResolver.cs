using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Backtesting;

public sealed class SubCandleResolver : ISubCandleResolver
{
    public SLTPResolution Resolve(
        decimal? stopLoss,
        decimal? takeProfit,
        Candle primaryCandle,
        IReadOnlyList<Candle>? subCandles)
    {
        if (!stopLoss.HasValue && !takeProfit.HasValue)
            return SLTPResolution.None;

        // ─── مسیر ۱: Sub-Candles موجود → دقیق ───
        if (subCandles is { Count: > 0 })
        {
            foreach (var sub in subCandles)
            {
                var slHit = stopLoss.HasValue && sub.Low <= stopLoss.Value;
                var tpHit = takeProfit.HasValue && sub.High >= takeProfit.Value;

                if (slHit && tpHit)
                {
                    // هر دو در یک Sub-Candle → Pessimistic (SL)
                    return new SLTPResolution(
                        true, stopLoss!.Value, BacktestExitReason.StopLoss);
                }
                if (slHit)
                {
                    return new SLTPResolution(
                        true, stopLoss!.Value, BacktestExitReason.StopLoss);
                }
                if (tpHit)
                {
                    return new SLTPResolution(
                        true, takeProfit!.Value, BacktestExitReason.TakeProfit);
                }
            }

            return SLTPResolution.None;
        }

        // ─── مسیر ۲: بدون Sub-Candle → Pessimistic روی کندل اصلی ───
        var pSlHit = stopLoss.HasValue && primaryCandle.Low <= stopLoss.Value;
        var pTpHit = takeProfit.HasValue && primaryCandle.High >= takeProfit.Value;

        if (pSlHit && pTpHit)
        {
            return new SLTPResolution(
                true, stopLoss!.Value, BacktestExitReason.StopLoss);
        }
        if (pSlHit)
        {
            return new SLTPResolution(
                true, stopLoss!.Value, BacktestExitReason.StopLoss);
        }
        if (pTpHit)
        {
            return new SLTPResolution(
                true, takeProfit!.Value, BacktestExitReason.TakeProfit);
        }

        return SLTPResolution.None;
    }
}