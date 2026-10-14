using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Runtime.Stages;

public sealed class MarketDataIngestionStage : IMarketDataIngestionStage
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly IMarketDataProviderResolver _providerResolver;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ILogger<MarketDataIngestionStage> _logger;

    public MarketDataIngestionStage(
        IRunExecutionContextCache runCache,
        IMarketDataProviderResolver providerResolver,
        IMarketSnapshotStore snapshotStore,
        ILogger<MarketDataIngestionStage> logger)
    {
        _runCache = runCache;
        _providerResolver = providerResolver;
        _snapshotStore = snapshotStore;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        var contexts = _runCache.GetAllActive();
        if (contexts.Count == 0)
        {
            // اگر هیچ Run فعالی نیست، Store را خالی نگه دار
            _snapshotStore.RetainOnly(new HashSet<string>());
            return;
        }

        // ─── ۱. اتحاد Symbols از همه‌ی Runهای فعال ───
        var isins = contexts
            .SelectMany(c => c.Symbols)
            .Select(s => s.Isin)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (isins.Count == 0) return;

        // ─── ۲. پاک‌سازی Snapshotهای بی‌استفاده ───
        _snapshotStore.RetainOnly(new HashSet<string>(isins, StringComparer.OrdinalIgnoreCase));

        // ─── ۳. گروه‌بندی بر اساس Market ───
        // در Phase 1 همه IranStock هستند. اگر Option هم اضافه شد،
        // اینجا باید Instrument را Query کنی تا Market هر Isin را بفهمی.
        var provider = _providerResolver.Resolve(MarketType.IranStock);
        if (provider is null)
        {
            _logger.LogWarning("No market data provider available for IranStock.");
            return;
        }

        // ─── ۴. Fetch و Store ───
        try
        {
            var snapshots = await provider.GetBatchSnapshotsAsync(isins, ct);
            if (snapshots.Count > 0)
                _snapshotStore.UpsertRange(snapshots);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Market data ingestion failed for {Count} symbols.", isins.Count);
        }
    }
}