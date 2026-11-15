using Trader.Application.Indicators.Ema;

namespace Trader.Application.Indicators.Macd;

/// <summary>
/// محاسبه‌ی خالص MACD. تنها وابستگی: EmaCalculator (pure).
///
/// MACD line = EMA(fast) - EMA(slow)
/// Signal line = EMA(MACD line, signalPeriod)
/// Histogram = MACD line - Signal line
/// </summary>
public static class MacdCalculator
{
    public static (
        IReadOnlyList<decimal?> MacdLine,
        IReadOnlyList<decimal?> SignalLine,
        IReadOnlyList<decimal?> Histogram) CalculateSeries(
        IReadOnlyList<decimal> values, MacdConfig config)
    {
        if (config.FastPeriod <= 0 || config.SlowPeriod <= 0 || config.SignalPeriod <= 0)
            throw new ArgumentOutOfRangeException(nameof(config),
                "All MACD periods must be > 0.");
        if (config.FastPeriod >= config.SlowPeriod)
            throw new ArgumentException(
                "FastPeriod must be < SlowPeriod.", nameof(config));

        var n = values.Count;

        var fastEma = EmaCalculator.CalculateSeries(values, config.FastPeriod);
        var slowEma = EmaCalculator.CalculateSeries(values, config.SlowPeriod);

        var macdLine = new List<decimal?>(n);
        for (int i = 0; i < n; i++)
        {
            if (fastEma[i] is null || slowEma[i] is null) macdLine.Add(null);
            else macdLine.Add(fastEma[i]!.Value - slowEma[i]!.Value);
        }

        // Signal = EMA of macdLine (starting from first non-null)
        int firstIdx = 0;
        while (firstIdx < n && macdLine[firstIdx] is null) firstIdx++;

        var signalLine = new List<decimal?>(n);
        for (int i = 0; i < firstIdx; i++) signalLine.Add(null);

        if (firstIdx < n)
        {
            var macdValues = new List<decimal>(n - firstIdx);
            for (int i = firstIdx; i < n; i++)
                macdValues.Add(macdLine[i]!.Value);

            var signalSeries = EmaCalculator.CalculateSeries(macdValues, config.SignalPeriod);
            for (int i = 0; i < signalSeries.Count; i++) signalLine.Add(signalSeries[i]);
        }

        var histogram = new List<decimal?>(n);
        for (int i = 0; i < n; i++)
        {
            if (macdLine[i] is null || signalLine[i] is null) histogram.Add(null);
            else histogram.Add(macdLine[i]!.Value - signalLine[i]!.Value);
        }

        return (macdLine, signalLine, histogram);
    }

    public static MacdResult? CalculateLast(
        IReadOnlyList<decimal> values, MacdConfig config)
    {
        var (macd, signal, hist) = CalculateSeries(values, config);
        if (macd.Count == 0) return null;
        if (macd[^1] is null || signal[^1] is null || hist[^1] is null) return null;
        return new MacdResult(macd[^1]!.Value, signal[^1]!.Value, hist[^1]!.Value);
    }
}