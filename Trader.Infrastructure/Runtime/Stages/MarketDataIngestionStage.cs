using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
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
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<MarketDataIngestionStage> _logger;

    public MarketDataIngestionStage(
        IRunExecutionContextCache runCache,
        IMarketDataProviderResolver providerResolver,
        IMarketSnapshotStore snapshotStore,
        ITraderUnitOfWork<TraderDbContext> uow,
        ITradingEventLogger eventLogger,
        ILogger<MarketDataIngestionStage> logger)
    {
        _runCache = runCache;
        _providerResolver = providerResolver;
        _snapshotStore = snapshotStore;
        _uow = uow;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        var contexts = _runCache.GetAllActive();
        if (contexts.Count == 0)
        {
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

            _eventLogger.Log(
                stage: TradingStage.MarketData,
                type: TradingEventType.MarketDataFetchFailed,
                severity: TradingEventSeverity.Error,
                message: "هیچ Provider داده‌ی بازار برای بورس ایران ثبت نشده است.");

            return;
        }

        // ─── ۳. Fetch ───
        var fetchStart = DateTime.UtcNow;
        IReadOnlyList<Application.Runtime.MarketSnapshot> fetched;

        try
        {
            fetched = await provider.GetBatchSnapshotsAsync(isins, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Market data fetch failed for {Count} symbols.", isins.Count);

            _eventLogger.Log(
                stage: TradingStage.MarketData,
                type: TradingEventType.MarketDataFetchFailed,
                severity: TradingEventSeverity.Error,
                message: $"دریافت قیمت {isins.Count} نماد از کارگزاری ناموفق بود: {ex.Message}",
                payload: new
                {
                    symbolCount = isins.Count,
                    errorType = ex.GetType().Name,
                    errorMessage = ex.Message,
                });

            return;
        }

        var fetchElapsed = (DateTime.UtcNow - fetchStart).TotalMilliseconds;

        // ─── ۴. اگر بازار بسته بود ───
        if (fetched.Count == 0)
        {
            _logger.LogDebug(
                "No snapshots returned for {Count} symbols (markets likely closed).",
                isins.Count);

            _eventLogger.Log(
                stage: TradingStage.MarketData,
                type: TradingEventType.MarketClosedSkipped,
                severity: TradingEventSeverity.Debug,
                message: $"قیمتی برای {isins.Count} نماد دریافت نشد " +
                         $"(بازار بسته یا نمادها غیرفعال).",
                payload: new
                {
                    requestedCount = isins.Count,
                    fetchedCount = 0,
                    elapsedMs = Math.Round(fetchElapsed, 1),
                });

            return;
        }

        // ─── ۵. Persist ───
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

        if (records.Count > 0)
        {
            try
            {
                await _uow.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to persist {Count} snapshots.", records.Count);

                _eventLogger.Log(
                    stage: TradingStage.MarketData,
                    type: TradingEventType.MarketDataFetchFailed,
                    severity: TradingEventSeverity.Error,
                    message: $"ذخیره‌ی {records.Count} Snapshot در دیتابیس ناموفق بود: {ex.Message}",
                    payload: new
                    {
                        recordCount = records.Count,
                        errorType = ex.GetType().Name,
                    });

                return;
            }
        }

        // ─── ۶. Update Store ───
        var idByIsin = records.ToDictionary(r => r.Isin, r => r.Id);
        var finalSnapshots = fetched
            .Where(s => idByIsin.ContainsKey(s.Isin))
            .Select(s => s with { Id = idByIsin[s.Isin] })
            .ToList();

        _snapshotStore.UpsertRange(finalSnapshots);

        // ─── ۷. MarketDataFetched Event ───
        _eventLogger.Log(
            stage: TradingStage.MarketData,
            type: TradingEventType.MarketDataFetched,
            severity: TradingEventSeverity.Info,
            message: $"قیمت {fetched.Count} از {isins.Count} نماد " +
                     $"از کارگزاری دریافت شد (مدت: {fetchElapsed:F0}ms).",
            payload: new
            {
                requestedCount = isins.Count,
                fetchedCount = fetched.Count,
                elapsedMs = Math.Round(fetchElapsed, 1),
            });

        _logger.LogDebug(
            "Stage 1: persisted {Count} snapshots.", finalSnapshots.Count);
    }
}