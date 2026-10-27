using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.MarketData;

public sealed class CurrencyCandleTransformer : ICandleTransformer
{
    private const string BaseCurrency = "IRR";

    /// <summary>
    /// Cache TTL — کوتاه تا وقتی نرخ جدید ثبت می‌شود، تأثیر بگیرد.
    /// </summary>
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private sealed record CacheEntry(
        IReadOnlyList<Candle> Transformed,
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

    public async Task<IReadOnlyList<Candle>> TransformAsync(
        string isin,
        IReadOnlyList<Candle> irrCandles,
        string targetCurrency,
        DateTime atTime,
        CancellationToken ct = default)
    {
        if (irrCandles.Count == 0) return irrCandles;

        var target = targetCurrency.Trim().ToUpperInvariant();
        if (target == BaseCurrency) return irrCandles;

        // ─── Cache Key: (isin, target, lastCandleTime) ───
        // وقتی کندل جدید می‌آید، lastCandleTime عوض می‌شود → Cache miss → Re-transform.
        var lastCandleTime = irrCandles[^1].OpenTime;
        var cacheKey = $"{isin}:{target}:{lastCandleTime:yyyyMMddHHmm}";

        if (_cache.TryGetValue(cacheKey, out var entry)
            && DateTime.UtcNow < entry.ExpiresAt)
        {
            return entry.Transformed;
        }

        // ─── Fetch Rate History ───
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

        // ─── Per-Candle Transform ───
        var baselineRate = rates[0].Rate;
        var firstRateTime = rates[0].EffectiveAt;
        var candlesBeforeFirstRate = 0;

        var result = new List<Candle>(irrCandles.Count);

        foreach (var candle in irrCandles)
        {
            var rate = FindRateAt(rates, candle.OpenTime);

            if (rate is null)
            {
                rate = baselineRate;
                candlesBeforeFirstRate++;
            }

            result.Add(candle with
            {
                Open = candle.Open * rate.Value,
                High = candle.High * rate.Value,
                Low = candle.Low * rate.Value,
                Close = candle.Close * rate.Value,
                // Volume دست‌نخورده — تعداد سهم
            });
        }

        if (candlesBeforeFirstRate > 0)
        {
            _logger.LogWarning(
                "{Count}/{Total} candles are OLDER than the first registered rate " +
                "for IRR→{Target} (first rate at {FirstRateTime:O}). " +
                "Using baseline rate {Baseline:E4} for them. " +
                "For accurate historical analysis, register rates covering the full candle range.",
                candlesBeforeFirstRate,
                irrCandles.Count,
                target,
                firstRateTime,
                baselineRate);
        }

        _logger.LogDebug(
            "Per-candle transform: {Count} candles IRR→{Target}, " +
            "ratePoints={RateCount}, baseline={Baseline:E4}, " +
            "candlesBeforeFirstRate={BeforeCount}",
            result.Count, target, rates.Count, baselineRate, candlesBeforeFirstRate);

        // ─── Update Cache ───
        _cache[cacheKey] = new CacheEntry(result, DateTime.UtcNow.Add(CacheTtl));

        return result;
    }

    /// <summary>
    /// آخرین نرخ با EffectiveAt ≤ atTime. Binary search روی آرایه‌ی صعودی.
    /// </summary>
    private static decimal? FindRateAt(
        IReadOnlyList<CurrencyRatePoint> rates, DateTime atTime)
    {
        int lo = 0, hi = rates.Count - 1;
        decimal? result = null;

        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;

            if (rates[mid].EffectiveAt <= atTime)
            {
                result = rates[mid].Rate;
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