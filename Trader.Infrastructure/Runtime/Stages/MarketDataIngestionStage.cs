using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.MarketData;

namespace Trader.Infrastructure.Runtime.Stages;

public sealed class MarketDataIngestionStage : IMarketDataIngestionStage
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly IMarketDataProviderResolver _providerResolver;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<MarketDataIngestionStage> _logger;

    public MarketDataIngestionStage(
        IRunExecutionContextCache runCache,
        IMarketDataProviderResolver providerResolver,
        IMarketSnapshotStore snapshotStore,
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<MarketDataIngestionStage> logger)
    {
        _runCache = runCache;
        _providerResolver = providerResolver;
        _snapshotStore = snapshotStore;
        _uow = uow;
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

        // ─── ۱. اتحاد Symbols ───
        var isins = contexts
            .SelectMany(c => c.Symbols)
            .Select(s => s.Isin)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (isins.Count == 0) return;

        _snapshotStore.RetainOnly(
            new HashSet<string>(isins, StringComparer.OrdinalIgnoreCase));

        // ─── ۲. Provider ───
        var provider = _providerResolver.Resolve(MarketType.IranStock);
        if (provider is null)
        {
            _logger.LogWarning("No market data provider for IranStock.");
            return;
        }

        // ─── ۳. Fetch ───
        IReadOnlyList<Application.Runtime.MarketSnapshot> fetched;
        try
        {
            fetched = await provider.GetBatchSnapshotsAsync(isins, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Market data fetch failed for {Count} symbols.", isins.Count);
            return;
        }

        if (fetched.Count == 0)
        {
            _logger.LogDebug(
                "No snapshots returned for {Count} symbols.", isins.Count);
            return;
        }

        // ─── ۴. Persist to DB ───
        var records = new List<Domain.Entities.MarketSnapshotRecord>(fetched.Count);
        foreach (var s in fetched)
        {
            try
            {
                var record = MarketSnapshotRecordMapper.FromRuntime(s);
                records.Add(record);
                await _uow.MarketSnapshots.AddAsync(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to map snapshot for {Isin}.", s.Isin);
            }
        }

        if (records.Count == 0) return;

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist {Count} snapshots.", records.Count);
            return;
        }

        // ─── ۵. Update Runtime Snapshots with Ids + Upsert ───
        var idByIsin = records.ToDictionary(r => r.Isin, r => r.Id);
        var finalSnapshots = fetched
            .Where(s => idByIsin.ContainsKey(s.Isin))
            .Select(s => s with { Id = idByIsin[s.Isin] })
            .ToList();

        _snapshotStore.UpsertRange(finalSnapshots);

        _logger.LogDebug(
            "Stage 1: persisted {Count} snapshots.", finalSnapshots.Count);
    }
}