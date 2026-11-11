using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Specifications.Candles;
using Trader.Application.Specifications.CurrencyRates;
using Trader.Domain.Entities;
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

    /* ═══════════════ Backfill ═══════════════ */

    public async Task CollectBackfillAsync(Guid jobId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId)
            ?? throw new InvalidOperationException($"Job {jobId} not found.");

        if (job.Mode != HistoricalCollectionMode.Backfill)
            throw new InvalidOperationException("CollectBackfillAsync requires Mode=Backfill.");

        if (job.State != HistoricalJobState.Running)
        {
            _logger.LogDebug("Job {Id} state={State}; skip.", jobId, job.State);
            return;
        }

        var isins = JsonSerializer.Deserialize<List<string>>(job.IsinsJson) ?? new();
        var tfs = JsonSerializer.Deserialize<List<int>>(job.TimeFramesJson) ?? new();
        var from = job.DateFrom;
        var to = job.DateTo ?? DateTime.UtcNow;

        _logger.LogInformation(
            "Backfill start: job={Id} isins={I} tfs={T} range={From}→{To}",
            jobId, isins.Count, tfs.Count, from, to);

        try
        {
            var totalCombinations = isins.Count * tfs.Count;
            var completed = 0;
            var totalInserted = 0;

            foreach (var isin in isins)
            {
                foreach (var tfInt in tfs)
                {
                    ct.ThrowIfCancellationRequested();
                    var tf = (TimeFrame)tfInt;

                    try
                    {
                        var (inserted, skipped) = await CollectComboAsync(
                            scope.ServiceProvider, isin, tf, from, to, ct);

                        totalInserted += inserted;
                        completed++;

                        job.RecordBackfillProgress(isin, tfInt, completed, totalInserted);
                        await uow.HistoricalDataCollectionJobs.UpdateAsync(job);
                        await uow.SaveChangesAsync(ct);

                        _logger.LogInformation(
                            "Combo done: job={JobId} isin={Isin} tf={Tf} " +
                            "inserted={Ins} skipped={Skp} ({Done}/{Total})",
                            jobId, isin, tf, inserted, skipped, completed, totalCombinations);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Combo failed: job={JobId} isin={Isin} tf={Tf}. Continuing.",
                            jobId, isin, tf);

                        completed++;
                        job.RecordBackfillProgress(isin, tfInt, completed, totalInserted);
                        await uow.HistoricalDataCollectionJobs.UpdateAsync(job);
                        await uow.SaveChangesAsync(ct);
                    }
                }
            }

            job.MarkCompleted(DateTime.UtcNow);
            await uow.HistoricalDataCollectionJobs.UpdateAsync(job);
            await uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Backfill completed: job={Id} totalInserted={Total}",
                jobId, totalInserted);
        }
        catch (OperationCanceledException)
        {
            job.MarkStopped(DateTime.UtcNow);
            await uow.HistoricalDataCollectionJobs.UpdateAsync(job);
            await uow.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            job.MarkFaulted(ex.Message);
            await uow.HistoricalDataCollectionJobs.UpdateAsync(job);
            await uow.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    /* ═══════════════ Recurring ═══════════════ */

    public async Task CollectRecurringAsync(Guid jobId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var job = await uow.HistoricalDataCollectionJobs.GetByIdAsync(jobId)
            ?? throw new InvalidOperationException($"Job {jobId} not found.");

        if (job.Mode != HistoricalCollectionMode.Recurring)
            throw new InvalidOperationException("CollectRecurringAsync requires Mode=Recurring.");

        if (job.State != HistoricalJobState.Running)
        {
            _logger.LogDebug("Job {Id} state={State}; skip.", jobId, job.State);
            return;
        }

        var isins = JsonSerializer.Deserialize<List<string>>(job.IsinsJson) ?? new();
        var tfs = JsonSerializer.Deserialize<List<int>>(job.TimeFramesJson) ?? new();
        var fallbackFrom = job.DateFrom;
        var now = DateTime.UtcNow;

        var totalInserted = 0;
        var totalSkipped = 0;

        try
        {
            foreach (var isin in isins)
            {
                foreach (var tfInt in tfs)
                {
                    ct.ThrowIfCancellationRequested();
                    var tf = (TimeFrame)tfInt;

                    try
                    {
                        // Boundary: max(OpenTime) برای این (Isin, TF) در DB
                        var boundary = await GetMaxCandleTimeAsync(uow, isin, tf, ct)
                            ?? fallbackFrom;

                        var (inserted, skipped) = await CollectComboAsync(
                            scope.ServiceProvider, isin, tf, boundary, now, ct);

                        totalInserted += inserted;
                        totalSkipped += skipped;

                        _logger.LogDebug(
                            "Recurring tick combo: job={JobId} isin={Isin} tf={Tf} " +
                            "boundary={Boundary} inserted={Ins} skipped={Skp}",
                            jobId, isin, tf, boundary, inserted, skipped);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Recurring tick combo failed: job={JobId} isin={Isin} tf={Tf}. Continuing.",
                            jobId, isin, tf);
                    }
                }
            }

            job.RecordRecurringRun(totalInserted, totalSkipped, DateTime.UtcNow);
            await uow.HistoricalDataCollectionJobs.UpdateAsync(job);
            await uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Recurring tick completed: job={Id} inserted={Ins} skipped={Skp}",
                jobId, totalInserted, totalSkipped);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Recurring: خطای یک tick نباید Job را MarkFaulted کند
            // (Scheduler خودش بعد از 10 failure Pause می‌کند)
            _logger.LogError(ex,
                "Recurring tick failed: job={Id}. Job remains Running; will retry next tick.",
                jobId);
            throw;
        }
    }

    /* ═══════════════ Shared — per combo ═══════════════ */

    private async Task<(int inserted, int skipped)> CollectComboAsync(
        IServiceProvider sp,
        string isin,
        TimeFrame tf,
        DateTime from,
        DateTime to,
        CancellationToken ct)
    {
        var providerResolver = sp.GetRequiredService<IMarketDataProviderResolver>();
        var persister = sp.GetRequiredService<ICandlePersistenceService>();

        var provider = providerResolver.Resolve(MarketType.IranStock)
            ?? throw new InvalidOperationException("No provider for IranStock.");

        var totalInserted = 0;
        var totalAttempted = 0;
        var currentTo = to;

        while (currentTo > from)
        {
            ct.ThrowIfCancellationRequested();

            IReadOnlyList<Trader.Application.Runtime.Candle> batch;
            try
            {
                batch = await provider.GetCandlesAsync(isin, tf, BatchSize, ct, currentTo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Batch fetch failed: isin={Isin} tf={Tf} to={To:O}. Stopping combo.",
                    isin, tf, currentTo);
                break;
            }

            if (batch.Count == 0) break;

            var filtered = batch
                .Where(c => c.OpenTime >= from && c.OpenTime <= to)
                .ToList();

            totalAttempted += filtered.Count;

            if (filtered.Count > 0)
            {
                var inserted = await persister.PersistAsync(
                    isin, (int)tf, filtered, provider.ProviderName, ct);
                totalInserted += inserted;
            }

            var oldest = batch.Min(c => c.OpenTime);
            if (oldest <= from) break;

            currentTo = oldest.AddTicks(-1);

            if (_options.RateLimitMs > 0)
                await Task.Delay(_options.RateLimitMs, ct);
        }

        return (totalInserted, totalAttempted - totalInserted);
    }

    private static async Task<DateTime?> GetMaxCandleTimeAsync(
        ITraderUnitOfWork<TraderDbContext> uow,
        string isin,
        TimeFrame tf,
        CancellationToken ct)
    {
        var rows = await uow.CandleRecords.GetAllAsync(q =>
            q.Where(c => c.Isin == isin && c.TimeFrame == tf)
             .OrderByDescending(c => c.OpenTime)
             .Take(1));

        return rows.FirstOrDefault()?.OpenTime;
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