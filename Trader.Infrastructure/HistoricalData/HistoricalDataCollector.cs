using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Abstractions.MarketData;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.HistoricalData;

public sealed class HistoricalDataCollector : IHistoricalDataCollector
{
    private const int BatchSize = 500;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HistoricalDataCollectionOptions _options;
    private readonly ILogger<HistoricalDataCollector> _logger;

    public HistoricalDataCollector(
        IServiceScopeFactory scopeFactory,
        IOptions<HistoricalDataCollectionOptions> options,
        ILogger<HistoricalDataCollector> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunAsync(Guid jobId, CancellationToken ct = default)
    {
        // ─── ۱. Load + Mark Running ───
        using (var scope = _scopeFactory.CreateScope())
        {
            var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
            var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId);
            if (job is null)
            {
                _logger.LogWarning("Job {JobId} not found.", jobId);
                return;
            }

            if (job.Status != CollectionJobStatus.Pending)
            {
                _logger.LogWarning("Job {JobId} status is {Status}, expected Pending.",
                    jobId, job.Status);
                return;
            }

            job.MarkRunning();
            await uow.SaveChangesAsync();
        }

        _logger.LogInformation("Collection job {JobId} started.", jobId);

        // ─── ۲. Process ───
        try
        {
            List<string> isins;
            List<int> timeFrames;
            DateTime dateFrom, dateTo;

            using (var scope = _scopeFactory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
                var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId)!;
                isins = JsonSerializer.Deserialize<List<string>>(job!.IsinsJson)!;
                timeFrames = JsonSerializer.Deserialize<List<int>>(job.TimeFramesJson)!;
                dateFrom = job.DateFrom;
                dateTo = job.DateTo;
            }

            int completed = 0;
            int totalCandles = 0;

            foreach (var isin in isins)
            {
                foreach (var tfInt in timeFrames)
                {
                    if (await IsCancelledAsync(jobId, ct))
                    {
                        _logger.LogInformation("Job {JobId} cancelled by user.", jobId);
                        return;
                    }

                    ct.ThrowIfCancellationRequested();

                    var tf = (TimeFrame)tfInt;

                    try
                    {
                        using var comboScope = _scopeFactory.CreateScope();
                        var collected = await CollectComboAsync(
                            comboScope.ServiceProvider, isin, tf, dateFrom, dateTo, ct);

                        totalCandles += collected;
                        completed++;

                        using (var progressScope = _scopeFactory.CreateScope())
                        {
                            var uow = progressScope.ServiceProvider
                                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
                            var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId);
                            if (job is null) return;

                            job.UpdateProgress(isin, tfInt, completed, totalCandles);
                            await uow.SaveChangesAsync();
                        }

                        _logger.LogInformation(
                            "Collected {Count} candles: isin={Isin} tf={Tf} ({Done}/{Total})",
                            collected, isin, tf, completed, isins.Count * timeFrames.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to collect {Isin} tf={Tf}. Continuing.",
                            isin, tf);

                        completed++;   // still count as attempted

                        using (var progressScope = _scopeFactory.CreateScope())
                        {
                            var uow = progressScope.ServiceProvider
                                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
                            var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId);
                            if (job is null) return;

                            job.UpdateProgress(isin, tfInt, completed, totalCandles);
                            await uow.SaveChangesAsync();
                        }
                    }
                }
            }

            // ─── ۳. Mark Completed ───
            using (var scope = _scopeFactory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
                var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId);
                if (job is null) return;

                job.MarkCompleted();
                await uow.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Job {JobId} completed. Total candles: {Total}.",
                jobId, totalCandles);
        }
        catch (OperationCanceledException)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
            var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId);
            job?.MarkCancelled();
            await uow.SaveChangesAsync();
            throw;
        }
        catch (Exception ex)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
            var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId);
            if (job is not null)
            {
                job.MarkFailed(ex.Message);
                await uow.SaveChangesAsync();
            }
            throw;
        }
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private async Task<bool> IsCancelledAsync(Guid jobId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId);
        return job?.Status == CollectionJobStatus.Cancelled;
    }

    private async Task<int> CollectComboAsync(
        IServiceProvider sp,
        string isin,
        TimeFrame timeFrame,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken ct)
    {
        var providerResolver = sp.GetRequiredService<IMarketDataProviderResolver>();
        var persister = sp.GetRequiredService<ICandlePersistenceService>();

        var provider = providerResolver.Resolve(MarketType.IranStock)
            ?? throw new InvalidOperationException("No provider for IranStock.");

        var collected = 0;
        var to = dateTo;
        var batchCount = 0;

        while (to > dateFrom)
        {
            ct.ThrowIfCancellationRequested();

            IReadOnlyList<Trader.Application.Runtime.Candle> batch;
            try
            {
                batch = await provider.GetCandlesAsync(isin, timeFrame, BatchSize, ct, to);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Batch fetch failed: isin={Isin} tf={Tf} to={To:O}. Stopping this combo.",
                    isin, timeFrame, to);
                break;
            }

            if (batch.Count == 0) break;

            batchCount++;

            var filtered = batch
                .Where(c => c.OpenTime >= dateFrom && c.OpenTime <= dateTo)
                .ToList();

            if (filtered.Count > 0)
            {
                var inserted = await persister.PersistAsync(
                    isin, (int)timeFrame, filtered, provider.ProviderName, ct);
                collected += inserted;
            }

            var oldest = batch.Min(c => c.OpenTime);
            if (oldest <= dateFrom) break;

            to = oldest.AddTicks(-1);

            if (_options.RateLimitMs > 0)
                await Task.Delay(_options.RateLimitMs, ct);
        }

        _logger.LogDebug(
            "Combo done: isin={Isin} tf={Tf} batches={Batches} collected={Collected}",
            isin, timeFrame, batchCount, collected);

        return collected;
    }
}

public sealed class HistoricalDataCollectionOptions
{
    public const string SectionName = "Trader:HistoricalData";

    /// <summary>تأخیر بین batchها (ms). ۲۰۰ = ~۵ req/s.</summary>
    public int RateLimitMs { get; set; } = 250;

    /// <summary>حداکثر روزهای گذشته که یک job می‌تواند داشته باشد.</summary>
    public int MaxDaysPerJob { get; set; } = 730;
}