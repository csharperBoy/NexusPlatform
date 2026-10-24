using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.MarketData;

public sealed class InMemoryCandleCache : ICandleCache
{
    private sealed record CacheEntry(
    IReadOnlyList<Candle> Candles,
    DateTime ExpiresAt,           // ← تغییر نام: LastClosedCandleTime → ExpiresAt
    DateTime FetchedAt);

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InMemoryCandleCache> _logger;

    public InMemoryCandleCache(
        IServiceScopeFactory scopeFactory,
        ILogger<InMemoryCandleCache> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Candle>> GetOrLoadAsync(
        string isin,
        TimeFrame timeFrame,
        int requiredCount,
        CancellationToken ct = default)
    {
        var key = BuildKey(isin, timeFrame);
        var now = DateTime.UtcNow;
        var tfDuration = timeFrame.ToDuration();

        // ─── Cache hit? ───
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Candles.Count >= requiredCount && now < entry.ExpiresAt)
            {
                return entry.Candles;
            }
        }

        // ─── Cache miss → Fetch ───
        IReadOnlyList<Candle> candles;
        string source = "BrokerMarketData";

        using (var scope = _scopeFactory.CreateScope())
        {
            var providerResolver = scope.ServiceProvider
                .GetRequiredService<IMarketDataProviderResolver>();

            var provider = providerResolver.Resolve(MarketType.IranStock);
            if (provider is null)
            {
                _logger.LogWarning("No provider for IranStock.");
                return Array.Empty<Candle>();
            }

            try
            {
                candles = await provider.GetCandlesAsync(
                    isin, timeFrame, requiredCount, ct);
                source = provider.ProviderName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to fetch candles for {Isin} tf={Tf}.",
                    isin, timeFrame);
                return Array.Empty<Candle>();
            }
        }

        if (candles.Count == 0) return Array.Empty<Candle>();

        // ─── Persist کندل‌های جدید ───
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var persister = scope.ServiceProvider
                .GetRequiredService<ICandlePersistenceService>();

            await persister.PersistAsync(
                isin, (int)timeFrame, candles, source, ct);
        }
        catch (Exception ex)
        {
            // Persistence fail → Cache را کنسل نمی‌کند
            _logger.LogWarning(ex,
                "Candle persistence failed for {Isin} tf={Tf}. Continuing with in-memory cache.",
                isin, timeFrame);
        }

        // ─── Expiry = زمان بسته شدن کندل بعدی + buffer ───
        var lastCandleTime = candles[^1].OpenTime;
        var nextCandleClose = lastCandleTime + tfDuration;

        // Buffer برای latency سرور — تا کندل جدید آماده شود
        // M1: 15s | M5: 30s | M15+: 60s | D1: 1h
        var buffer = timeFrame switch
        {
            TimeFrame.M1 => TimeSpan.FromSeconds(15),
            TimeFrame.M5 => TimeSpan.FromSeconds(30),
            TimeFrame.M15 => TimeSpan.FromSeconds(60),
            TimeFrame.M30 => TimeSpan.FromSeconds(90),
            TimeFrame.H1 => TimeSpan.FromMinutes(3),
            TimeFrame.H4 => TimeSpan.FromMinutes(5),
            TimeFrame.D1 => TimeSpan.FromHours(2),      // شب تا صبح cache
            TimeFrame.W1 => TimeSpan.FromHours(12),
            _ => TimeSpan.FromSeconds(30)
        };

        var expiresAt = nextCandleClose + buffer;
        _cache[key] = new CacheEntry(candles, expiresAt, now);

        _logger.LogDebug(
            "Candles loaded: {Isin} tf={Tf} count={Count} lastCandle={Last} expiresAt={Exp}",
            isin, timeFrame, candles.Count, lastCandleTime, expiresAt);

        return candles;
    }

    public void Invalidate(string isin, TimeFrame timeFrame)
    {
        _cache.TryRemove(BuildKey(isin, timeFrame), out _);
    }

    public void InvalidateAll() => _cache.Clear();

    private static string BuildKey(string isin, TimeFrame tf) => $"{isin}:{(int)tf}";
}