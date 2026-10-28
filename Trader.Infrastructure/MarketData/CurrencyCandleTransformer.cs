using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.MarketData;

public sealed class CurrencyCandleTransformer : ICandleTransformer
{
    private const string BaseCurrency = "IRR";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private sealed record CacheEntry(
        CandleTransformationResult Result,
        DateTime ExpiresAt);

    private readonly ICurrencyRateProvider _rateProvider;
    private readonly ILogger<CurrencyCandleTransformer> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public CurrencyCandleTransformer(
        ICurrencyRateProvider rateProvider,
        ILogger<CurrencyCandleTransformer> logger)
    {
        _rateProvider = rateProvider;
        _logger = logger;
    }

    public async Task<CandleTransformationResult> TransformAsync(
        string isin,
        IReadOnlyList<Candle> irrCandles,
        string targetCurrency,
        DateTime atTime,
        CancellationToken ct = default)
    {
        if (irrCandles.Count == 0)
        {
            return new CandleTransformationResult(
                irrCandles, null, null, 0, 0);
        }

        var target = targetCurrency.Trim().ToUpperInvariant();
        if (target == BaseCurrency)
        {
            return new CandleTransformationResult(
                irrCandles, 1m, null, 0, 0);
        }

        var lastCandleTime = irrCandles[^1].OpenTime;
        var cacheKey = $"{isin}:{target}:{lastCandleTime:yyyyMMddHHmm}";

        if (_cache.TryGetValue(cacheKey, out var entry)
            && DateTime.UtcNow < entry.ExpiresAt)
        {
            return entry.Result;
        }

        IReadOnlyList<CurrencyRatePoint> rates;
        try
        {
            rates = await _rateProvider.GetRateHistoryAsync(
                BaseCurrency, target, atTime, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to fetch rate history for {BaseCurrency}→{target}: {ex.Message}", ex);
        }

        if (rates.Count == 0)
        {
            throw new InvalidOperationException(
                $"No currency rates registered for {BaseCurrency}→{target} " +
                $"up to {atTime:O}. Register at least one rate via /trader/currency-rates.");
        }

        // ─── Debug: نمایش دقیق نقاط نرخ ───
        _logger.LogDebug(
            "Rate points for {From}→{To}: [{Points}]",
            BaseCurrency, target,
            string.Join(", ", rates.Select(r =>
                $"{r.Rate:E8}@{r.EffectiveAt:yyyy-MM-dd HH:mm:ssZ}")));

        var baselineRate = rates[0].Rate;
        var firstRateTime = rates[0].EffectiveAt;
        var candlesBeforeFirstRate = 0;

        var result = new List<Candle>(irrCandles.Count);

        decimal? lastCandleRate = null;
        DateTime? lastCandleRateEffectiveAt = null;

        for (int i = 0; i < irrCandles.Count; i++)
        {
            var candle = irrCandles[i];
            var isLastCandle = (i == irrCandles.Count - 1);

            // ─── کندل آخر (in-progress): از atTime (الان) ───
            // ─── کندل‌های تاریخی: از OpenTime ───
            var lookupTime = isLastCandle ? atTime : candle.OpenTime;

            var ratePoint = FindRateAt(rates, lookupTime);

            decimal rate;
            if (ratePoint is null)
            {
                rate = baselineRate;
                candlesBeforeFirstRate++;
            }
            else
            {
                rate = ratePoint.Rate;
            }

            if (i == irrCandles.Count - 1)
            {
                lastCandleRate = rate;
                lastCandleRateEffectiveAt = ratePoint?.EffectiveAt ?? firstRateTime;
            }

            result.Add(candle with
            {
                Open = candle.Open * rate,
                High = candle.High * rate,
                Low = candle.Low * rate,
                Close = candle.Close * rate,
            });
        }

        if (candlesBeforeFirstRate > 0)
        {
            _logger.LogWarning(
                "{Count}/{Total} candles are OLDER than the first registered rate " +
                "for IRR→{Target} (first rate at {FirstRateTime:O}). " +
                "Using baseline rate {Baseline:E8} for them.",
                candlesBeforeFirstRate, irrCandles.Count, target,
                firstRateTime, baselineRate);
        }

        _logger.LogDebug(
            "Per-candle transform: {Count} candles IRR→{Target}, ratePoints={RateCount}, " +
            "lastCandleRate={LastRate:E8} (effective at {LastEffectiveAt:O}), " +
            "candlesBeforeFirstRate={BeforeCount}",
            result.Count, target, rates.Count,
            lastCandleRate, lastCandleRateEffectiveAt, candlesBeforeFirstRate);

        var finalResult = new CandleTransformationResult(
            result,
            lastCandleRate,
            lastCandleRateEffectiveAt,
            candlesBeforeFirstRate,
            rates.Count);

        _cache[cacheKey] = new CacheEntry(finalResult, DateTime.UtcNow.Add(CacheTtl));

        return finalResult;
    }

    private static CurrencyRatePoint? FindRateAt(
        IReadOnlyList<CurrencyRatePoint> rates, DateTime atTime)
    {
        int lo = 0, hi = rates.Count - 1;
        CurrencyRatePoint? result = null;

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (rates[mid].EffectiveAt <= atTime)
            {
                result = rates[mid];
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        return result;
    }
}