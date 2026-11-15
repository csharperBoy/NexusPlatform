namespace Trader.Application.Indicators.Ema;

/// <summary>
/// محاسبه‌ی خالص EMA. بدون حالت، بدون I/O.
/// فرمول: EMA[0..period-2] = null، EMA[period-1] = SMA(period)،
///       EMA[i] = (value[i] - EMA[i-1]) × k + EMA[i-1] ، k = 2/(period+1)
/// </summary>
public static class EmaCalculator
{
    public static IReadOnlyList<decimal?> CalculateSeries(
        IReadOnlyList<decimal> values, int period)
    {
        if (period <= 0)
            throw new ArgumentOutOfRangeException(nameof(period),
                "EMA period must be greater than zero.");

        var result = new List<decimal?>(values.Count);

        if (values.Count < period)
        {
            for (int i = 0; i < values.Count; i++) result.Add(null);
            return result;
        }

        // Seed = SMA(period)
        decimal sum = 0m;
        for (int i = 0; i < period; i++) sum += values[i];
        var seed = sum / period;

        for (int i = 0; i < period - 1; i++) result.Add(null);
        result.Add(seed);

        var k = 2m / (period + 1m);
        var prev = seed;

        for (int i = period; i < values.Count; i++)
        {
            var ema = (values[i] - prev) * k + prev;
            result.Add(ema);
            prev = ema;
        }

        return result;
    }

    public static decimal? CalculateLast(
        IReadOnlyList<decimal> values, int period)
    {
        var series = CalculateSeries(values, period);
        return series.Count == 0 ? null : series[^1];
    }
}