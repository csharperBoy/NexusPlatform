using Trader.Application.Runtime;

namespace Trader.Application.Indicators.Atr;

/// <summary>
/// محاسبه‌ی خالص ATR با Wilder's Smoothing.
///
/// TR[i] = max(High[i]-Low[i], |High[i]-Close[i-1]|, |Low[i]-Close[i-1]|)
/// ATR[period] = mean(TR[1..period])
/// ATR[i]       = (ATR[i-1] × (period-1) + TR[i]) / period
/// </summary>
public static class AtrCalculator
{
    public static decimal? Calculate(IReadOnlyList<Candle> candles, int period)
    {
        if (period <= 0)
            throw new ArgumentOutOfRangeException(nameof(period),
                "ATR period must be greater than zero.");

        if (candles.Count < period + 1)
            return null;

        // TR[i-1] برای i = 1..n-1
        var trs = new decimal[candles.Count - 1];
        for (int i = 1; i < candles.Count; i++)
        {
            var high = candles[i].High;
            var low = candles[i].Low;
            var prevClose = candles[i - 1].Close;

            var hl = high - low;
            var hc = Math.Abs(high - prevClose);
            var lc = Math.Abs(low - prevClose);

            trs[i - 1] = Math.Max(hl, Math.Max(hc, lc));
        }

        // Seed = simple mean of first `period` TRs
        decimal sum = 0m;
        for (int i = 0; i < period; i++) sum += trs[i];
        var atr = sum / period;

        // Wilder's smoothing
        for (int i = period; i < trs.Length; i++)
        {
            atr = (atr * (period - 1) + trs[i]) / period;
        }

        return atr;
    }
}