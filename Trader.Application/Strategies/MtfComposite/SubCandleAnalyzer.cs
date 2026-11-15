using Trader.Application.Runtime;

namespace Trader.Application.Strategies.MtfComposite;

public sealed record SubCandleAnalysisResult(
    bool Triggered,
    decimal VolumeRatio,
    decimal CurrentVolume,
    decimal ExpectedVolume,
    decimal OpeningBaseline,
    decimal IntraDayBaseline,
    bool IsOpening);

/// <summary>
/// منطق خالص تحلیل Intra-Candle.
///
/// ورودی: sub-candle ها + موقعیت فعلی در primary candle
/// خروجی: آیا حجم نیمه‌ی اول کندل جاری به اندازه‌ی کافی بالاست (نسبت به baseline)؟
///
/// Baselineها از sub-candle های تاریخی محاسبه می‌شن:
///   • IntraDay: کندل‌های primary غیر-اول-روز
///   • Opening: کندل‌های primary اول-روز (فاصله > ۶ ساعت از قبلی)
/// </summary>
public static class SubCandleAnalyzer
{
    public static SubCandleAnalysisResult? Analyze(
        IReadOnlyList<Candle> subCandles,
        TimeSpan subDuration,
        DateTime currentPrimaryCandleStart,
        TimeSpan primaryDuration,
        DateTime nowUtc,
        int openingBaselineDays,
        int intraDayBaselineCandles,
        int minSamples,
        decimal triggerThreshold)
    {
        if (subCandles.Count == 0) return null;
        if (subDuration <= TimeSpan.Zero) return null;

        var halfDuration = primaryDuration / 2;
        if (halfDuration <= TimeSpan.Zero) return null;

        // گروه‌بندی sub-candles بر اساس primary candle
        var groups = new Dictionary<DateTime, List<Candle>>();
        foreach (var sub in subCandles)
        {
            var primaryStart = AlignToPrimary(sub.OpenTime, primaryDuration);
            if (!groups.TryGetValue(primaryStart, out var list))
            {
                list = new List<Candle>();
                groups[primaryStart] = list;
            }
            list.Add(sub);
        }

        // محاسبه‌ی first-half volumes برای کندل‌های تاریخی
        var intraDayFirstHalf = new List<decimal>();
        var openingFirstHalf = new List<decimal>();

        var sortedStarts = groups.Keys.OrderBy(k => k).ToList();
        DateTime? prevGroupStart = null;

        foreach (var groupStart in sortedStarts)
        {
            if (groupStart >= currentPrimaryCandleStart) continue;

            var gap = prevGroupStart.HasValue
                ? groupStart - prevGroupStart.Value
                : TimeSpan.MaxValue;
            var isOpening = gap > TimeSpan.FromHours(6);
            prevGroupStart = groupStart;

            var halfEnd = groupStart + halfDuration;
            decimal vol = 0m;
            int cnt = 0;
            foreach (var c in groups[groupStart])
            {
                if (c.OpenTime >= groupStart && c.OpenTime < halfEnd)
                {
                    vol += c.Volume;
                    cnt++;
                }
            }
            if (cnt == 0) continue;

            if (isOpening) openingFirstHalf.Add(vol);
            else intraDayFirstHalf.Add(vol);
        }

        // Baseline معتبر؟
        var minSamplesEff = Math.Max(1, minSamples);

        var intraDayTrimmed = intraDayFirstHalf
            .TakeLast(Math.Max(1, intraDayBaselineCandles)).ToList();
        var openingTrimmed = openingFirstHalf
            .TakeLast(Math.Max(1, openingBaselineDays)).ToList();

        var intraDayBaseline = intraDayTrimmed.Count >= minSamplesEff
            ? Median(intraDayTrimmed) : 0m;
        var openingBaseline = openingTrimmed.Count >= minSamplesEff
            ? Median(openingTrimmed) : 0m;

        // تشخیص Opening برای کندل جاری
        var currentIsOpening = !prevGroupStart.HasValue
            || (currentPrimaryCandleStart - prevGroupStart.Value) > TimeSpan.FromHours(6);

        var selectedBaseline = currentIsOpening ? openingBaseline : intraDayBaseline;
        if (selectedBaseline <= 0m) return null;

        // حجم نیمه‌ی اول کندل جاری (فقط sub-candle های بسته شده)
        var elapsed = nowUtc - currentPrimaryCandleStart;
        if (elapsed <= TimeSpan.Zero || elapsed >= halfDuration) return null;

        var currentHalfEnd = currentPrimaryCandleStart + halfDuration;
        decimal currentVol = 0m;
        int currentCnt = 0;
        foreach (var sub in subCandles)
        {
            if (sub.OpenTime < currentPrimaryCandleStart) continue;
            if (sub.OpenTime >= currentHalfEnd) continue;
            if (sub.OpenTime + subDuration > nowUtc) continue; // in-progress
            currentVol += sub.Volume;
            currentCnt++;
        }
        if (currentCnt == 0) return null;

        var timeFraction = (decimal)elapsed.TotalSeconds
                         / (decimal)halfDuration.TotalSeconds;
        if (timeFraction <= 0m) return null;

        var expectedVolume = selectedBaseline * timeFraction;
        if (expectedVolume <= 0m) return null;

        var ratio = currentVol / expectedVolume;

        return new SubCandleAnalysisResult(
            Triggered: ratio >= triggerThreshold,
            VolumeRatio: ratio,
            CurrentVolume: currentVol,
            ExpectedVolume: expectedVolume,
            OpeningBaseline: openingBaseline,
            IntraDayBaseline: intraDayBaseline,
            IsOpening: currentIsOpening);
    }

    private static DateTime AlignToPrimary(DateTime subOpenTime, TimeSpan primaryDuration)
    {
        var period = primaryDuration.Ticks;
        var ticks = subOpenTime.Ticks;
        return new DateTime(ticks - (ticks % period), subOpenTime.Kind);
    }

    private static decimal Median(List<decimal> values)
    {
        if (values.Count == 0) return 0m;
        var sorted = values.OrderBy(x => x).ToList();
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2m
            : sorted[mid];
    }
}