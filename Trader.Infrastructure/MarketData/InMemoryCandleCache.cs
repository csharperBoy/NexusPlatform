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
        DateTime ExpiresAt,
        DateTime FetchedAt);

    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITradingScheduleCache _scheduleCache;
    private readonly IMarketSessionProvider _marketSession;
    private readonly ILogger<InMemoryCandleCache> _logger;

    public InMemoryCandleCache(
        IServiceScopeFactory scopeFactory,
        ITradingScheduleCache scheduleCache,
        IMarketSessionProvider marketSession,
        ILogger<InMemoryCandleCache> logger)
    {
        _scopeFactory = scopeFactory;
        _scheduleCache = scheduleCache;
        _marketSession = marketSession;
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

        // ─── Cache hit? ───
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Candles.Count >= requiredCount && now < entry.ExpiresAt)
                return entry.Candles;
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
                candles = await provider.GetCandlesAsync(isin, timeFrame, requiredCount, ct);
                source = provider.ProviderName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to fetch candles for {Isin} tf={Tf}.", isin, timeFrame);
                return Array.Empty<Candle>();
            }
        }

        if (candles.Count == 0) return Array.Empty<Candle>();

        // ─── Persist ───
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var persister = scope.ServiceProvider.GetRequiredService<ICandlePersistenceService>();
            await persister.PersistAsync(isin, (int)timeFrame, candles, source, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Candle persistence failed for {Isin} tf={Tf}. Continuing.", isin, timeFrame);
        }

        // ─── Expiry با آگاهی از Schedule ───
        var expiresAt = await ComputeExpiryAsync(isin, timeFrame, candles, now, ct);
        _cache[key] = new CacheEntry(candles, expiresAt, now);

        _logger.LogDebug(
            "Candles loaded: {Isin} tf={Tf} count={Count} lastOpen={Last} expiresAt={Exp}",
            isin, timeFrame, candles.Count, candles[^1].OpenTime, expiresAt);

        return candles;
    }

    public void Invalidate(string isin, TimeFrame timeFrame)
        => _cache.TryRemove(BuildKey(isin, timeFrame), out _);

    public void InvalidateAll() => _cache.Clear();

    private static string BuildKey(string isin, TimeFrame tf) => $"{isin}:{(int)tf}";

    /* ═══════════════════ Expiry ═══════════════════ */

    private async Task<DateTime> ComputeExpiryAsync(
        string isin,
        TimeFrame timeFrame,
        IReadOnlyList<Candle> candles,
        DateTime now,
        CancellationToken ct)
    {
        var buffer = BufferFor(timeFrame);
        var schedule = await _scheduleCache.GetForIsinAsync(isin, ct);

        // ─── بازار بسته: تا باز شدن بعدی ───
        if (!_marketSession.IsMarketOpen(schedule, now))
        {
            var nextOpen = _marketSession.GetNextMarketOpen(schedule, now);
            return nextOpen + buffer;
        }

        // ─── بازار باز: کِی کندل بعدی بسته می‌شود؟ ───
        var tfDuration = timeFrame.ToDuration();
        var lastCandleOpen = candles[^1].OpenTime;
        var lastCandleClose = lastCandleOpen + tfDuration;

        if (lastCandleClose > now)
            return lastCandleClose + buffer;

        var tfTicks = tfDuration.Ticks;
        var nextBoundaryTicks = ((now.Ticks / tfTicks) + 1) * tfTicks;
        var nextBoundary = new DateTime(nextBoundaryTicks, DateTimeKind.Utc);

        return nextBoundary + buffer;
    }

    private static TimeSpan BufferFor(TimeFrame tf) => tf switch
    {
        TimeFrame.M1 => TimeSpan.FromSeconds(15),
        TimeFrame.M5 => TimeSpan.FromSeconds(30),
        TimeFrame.M15 => TimeSpan.FromSeconds(60),
        TimeFrame.M30 => TimeSpan.FromSeconds(90),
        TimeFrame.H1 => TimeSpan.FromMinutes(3),
        TimeFrame.H4 => TimeSpan.FromMinutes(5),
        TimeFrame.D1 => TimeSpan.FromMinutes(30),
        TimeFrame.W1 => TimeSpan.FromHours(2),
        _ => TimeSpan.FromSeconds(30)
    };
}