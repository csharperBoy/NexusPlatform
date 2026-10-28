namespace Trader.Application.Indicators.Rsi;

/// <summary>محاسبه‌ی خالص RSI. بدون حالت، بدون I/O.</summary>
public static class RsiCalculator
{
    /// <summary>
    /// RSI را با روش Wilder's Smoothing محاسبه می‌کند.
    /// خروجی: مقدار آخرین RSI، یا null اگر داده کافی نبود.
    /// </summary>
    public static decimal? Calculate(IReadOnlyList<decimal> closes, int period)
    {
        if (period <= 0 || closes.Count < period + 1)
            return null;

        decimal gainSum = 0m;
        decimal lossSum = 0m;

        // اولین میانگین ساده
        for (int i = 1; i <= period; i++)
        {
            var diff = closes[i] - closes[i - 1];
            if (diff > 0) gainSum += diff;
            else lossSum -= diff;
        }

        var avgGain = gainSum / period;
        var avgLoss = lossSum / period;

        // Wilder's smoothing برای بقیه
        for (int i = period + 1; i < closes.Count; i++)
        {
            var diff = closes[i] - closes[i - 1];
            var gain = diff > 0 ? diff : 0m;
            var loss = diff < 0 ? -diff : 0m;

            avgGain = ((avgGain * (period - 1)) + gain) / period;
            avgLoss = ((avgLoss * (period - 1)) + loss) / period;
        }

        if (avgLoss == 0m)
        {
            // همه صعود یا همه صاف
            return avgGain == 0m ? 50m : 100m;
        }

        var rs = avgGain / avgLoss;
        var rsi = 100m - (100m / (1m + rs));
        return rsi;
    }
}