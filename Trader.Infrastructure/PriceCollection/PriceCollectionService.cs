using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.ValueObjects;
using System.Text.Json;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Dtos.PriceCollection;
using Trader.Application.Scheduler;
using Trader.Application.Specifications.CurrencyRates;
using Trader.Application.Specifications.PriceCollection;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.PriceCollection.Tgju;

namespace Trader.Infrastructure.PriceCollection;

public sealed class PriceCollectionService
    : IPriceCollectionService,
      IPriceSourceQueryService,
      IPriceCollectionJobQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IPriceSourceProviderResolver _providerResolver;
    private readonly ISchedulerService _scheduler;
    private readonly PriceCollectionOptions _opts;
    private readonly ILogger<PriceCollectionService> _logger;

    private static readonly TimeZoneInfo TehranTz = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

    public PriceCollectionService(
        ITraderUnitOfWork<TraderDbContext> uow,
        IPriceSourceProviderResolver providerResolver,
        ISchedulerService scheduler,
        IOptions<PriceCollectionOptions> opts,
        ILogger<PriceCollectionService> logger)
    {
        _uow = uow;
        _providerResolver = providerResolver;
        _scheduler = scheduler;
        _opts = opts.Value;
        _logger = logger;
    }

    /* ═══════════════════ PriceSource CRUD ═══════════════════ */

    public async Task<Guid> CreateSourceAsync(CreatePriceSourceModel model)
    {
        var existing = await _uow.PriceSourceSpecs.GetBySpecAsync(
    new PriceSourceByCodeSpec(model.Code.ToLowerInvariant()));
        if (existing is not null)
            throw new InvalidOperationException(
                $"PriceSource with Code '{model.Code}' already exists.");

        var source = PriceSource.Create(
            model.Code, model.Name, (PriceSourceType)model.Type,
            model.ConfigJson, model.FromAsset, model.ToAsset);

        await _uow.PriceSources.AddAsync(source);
        _logger.LogInformation("PriceSource created: {Id} code={Code}", source.Id, source.Code);
        return source.Id;
    }

    public async Task UpdateSourceAsync(Guid id, CreatePriceSourceModel model)
    {
        var source = await _uow.PriceSources.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"PriceSource {id} not found.");

        source.Update(model.Name, model.ConfigJson, model.FromAsset, model.ToAsset);
        await _uow.PriceSources.UpdateAsync(source);
    }

    public async Task DeactivateSourceAsync(Guid id)
    {
        var source = await _uow.PriceSources.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"PriceSource {id} not found.");

        source.Deactivate();
        await _uow.PriceSources.UpdateAsync(source);
    }

    /* ═══════════════════ Job Create/Start/Stop ═══════════════════ */

    public async Task<Guid> CreateJobAsync(CreatePriceCollectionJobModel model)
    {
        var source = await _uow.PriceSources.GetByIdAsync(model.PriceSourceId)
            ?? throw new InvalidOperationException(
                $"PriceSource {model.PriceSourceId} not found.");

        if (!source.IsActive)
            throw new InvalidOperationException(
                $"PriceSource '{source.Code}' is inactive.");

        var mode = (PriceCollectionMode)model.Mode;

        var job = mode switch
        {
            PriceCollectionMode.Recurring => PriceCollectionJob.CreateRecurring(
                source.Id,
                model.RecurrencePatternJson
                    ?? throw new InvalidOperationException(
                        "RecurrencePatternJson required for Recurring mode.")),
            PriceCollectionMode.Backfill => PriceCollectionJob.CreateBackfill(source.Id),
            _ => throw new InvalidOperationException($"Invalid mode {model.Mode}."),
        };

        await _uow.PriceCollectionJobs.AddAsync(job);
        _logger.LogInformation(
            "PriceCollectionJob created: {Id} source={Code} mode={Mode}",
            job.Id, source.Code, mode);
        return job.Id;
    }

    public async Task StartJobAsync(Guid id)
    {
        var job = await _uow.PriceCollectionJobs.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"PriceCollectionJob {id} not found.");

        if (job.State == PriceCollectionJobState.Running)
            throw new InvalidOperationException("Job is already Running.");

        var source = await _uow.PriceSources.GetByIdAsync(job.PriceSourceId)
            ?? throw new InvalidOperationException("PriceSource not found.");

        if (!source.IsActive)
            throw new InvalidOperationException($"PriceSource '{source.Code}' is inactive.");

        var provider = _providerResolver.Resolve(source.Type)
            ?? throw new InvalidOperationException(
                $"No Provider registered for SourceType={source.Type}.");

        if (job.Mode == PriceCollectionMode.Recurring)
        {
            var pattern = RecurrencePattern.FromJson(job.RecurrencePatternJson!);

            var schedulerId = await _scheduler.ScheduleRecurringAsync(
                name: $"price-collect-{job.Id}",
                payload: new CollectPricePayload(job.Id),
                pattern: pattern,
                startAt: DateTimeOffset.UtcNow.AddMinutes(1),
                endAt: null,
                maxOccurrences: null,
                maxFailures: 10,
                timeZone: TehranTz,
                queue: "scheduler");

            job.MarkRunningRecurring(schedulerId, DateTime.UtcNow);
            await _uow.PriceCollectionJobs.UpdateAsync(job);

            _logger.LogInformation(
                "PriceCollectionJob {Id} started (Recurring). SchedulerRecurringId={RecId}",
                job.Id, schedulerId);
        }
        else // Backfill
        {
            var schedulerJobId = await _scheduler.ScheduleAsync(
                payload: new CollectPricePayload(job.Id),
                fireAt: DateTimeOffset.UtcNow,
                queue: "scheduler");

            job.MarkRunningBackfill(schedulerJobId, DateTime.UtcNow);
            await _uow.PriceCollectionJobs.UpdateAsync(job);

            _logger.LogInformation(
                "PriceCollectionJob {Id} started (Backfill). SchedulerJobId={JobId}",
                job.Id, schedulerJobId);
        }
    }

    public async Task StopJobAsync(Guid id)
    {
        var job = await _uow.PriceCollectionJobs.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"PriceCollectionJob {id} not found.");

        if (job.State != PriceCollectionJobState.Running)
            return; // no-op

        if (job.Mode == PriceCollectionMode.Recurring && job.SchedulerRecurringId.HasValue)
        {
            try
            {
                await _scheduler.CancelRecurringAsync(job.SchedulerRecurringId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to cancel SchedulerRecurring {RecId}. DB will still be marked Stopped.",
                    job.SchedulerRecurringId.Value);
            }
        }
        else if (job.Mode == PriceCollectionMode.Backfill && job.SchedulerJobId.HasValue)
        {
            try
            {
                await _scheduler.CancelAsync(job.SchedulerJobId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to cancel SchedulerJob {JobId}. DB will still be marked Stopped.",
                    job.SchedulerJobId.Value);
            }
        }

        job.MarkStopped(DateTime.UtcNow);
        await _uow.PriceCollectionJobs.UpdateAsync(job);

        _logger.LogInformation("PriceCollectionJob {Id} stopped.", job.Id);
    }

    /* ═══════════════════ Collect — called from Handler ═══════════════════ */

    public async Task CollectOnceAsync(Guid jobId, CancellationToken ct = default)
    {
        // Recurring: page 1 only
        await CollectInternalAsync(jobId, maxPages: 1, isBackfill: false, ct);
    }

    public async Task CollectBackfillAsync(Guid jobId, CancellationToken ct = default)
    {
        await CollectInternalAsync(jobId, maxPages: int.MaxValue, isBackfill: true, ct);
    }

    private async Task CollectInternalAsync(
        Guid jobId, int maxPages, bool isBackfill, CancellationToken ct)
    {
        var job = await _uow.PriceCollectionJobs.GetByIdAsync(jobId)
            ?? throw new InvalidOperationException($"Job {jobId} not found.");

        if (job.State != PriceCollectionJobState.Running)
        {
            _logger.LogDebug("Job {Id} state={State}; skip.", jobId, job.State);
            return;
        }

        var source = await _uow.PriceSources.GetByIdAsync(job.PriceSourceId)
            ?? throw new InvalidOperationException("PriceSource not found.");

        var provider = _providerResolver.Resolve(source.Type)
            ?? throw new InvalidOperationException(
                $"No Provider for SourceType={source.Type}.");

        // Determine max(EffectiveAt) برای boundary
        var maxDb = await GetMaxEffectiveAtAsync(source.FromAsset, source.ToAsset, source.Code);

        var fetch = await provider.FetchAsync(source, maxDb, maxPages, ct);

        if (fetch.Points.Count == 0)
        {
            _logger.LogDebug("Job {Id}: no new points (hitBoundary={Hit}).",
                jobId, fetch.HitBoundary);
            job.RecordRun(0, 0, DateTime.UtcNow);

            if (isBackfill)
                job.MarkCompleted(DateTime.UtcNow);

            await _uow.PriceCollectionJobs.UpdateAsync(job);
            await _uow.SaveChangesAsync(ct);
            return;
        }

        // Persist با sanity + dedupe
        var (inserted, skipped) = await PersistAsync(
            source, fetch.Points, job, isBackfill, ct);

        job.RecordRun(inserted, skipped, DateTime.UtcNow);

        if (isBackfill && fetch.HitBoundary)
            job.MarkCompleted(DateTime.UtcNow);

        await _uow.PriceCollectionJobs.UpdateAsync(job);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Job {Id} source={Code} inserted={Ins} skipped={Skp} mode={Mode}",
            jobId, source.Code, inserted, skipped, isBackfill ? "Backfill" : "Recurring");
    }

    /* ═══════════════════ Persist + Sanity ═══════════════════ */

    private async Task<(int inserted, int skipped)> PersistAsync(
        PriceSource source,
        IReadOnlyList<PricePoint> points,
        PriceCollectionJob job,
        bool isBackfill,
        CancellationToken ct)
    {
        if (points.Count == 0) return (0, 0);

        var from = source.FromAsset.ToUpperInvariant();
        var to = source.ToAsset.ToUpperInvariant();

        // Config bounds (per-source + global fallback)
        var (minRate, maxRate, maxDailyChangePct) =
            ReadSanityBounds(source, out _);

        // Pre-load existing EffectiveAt در بازه
        var minTime = points.Min(p => p.EffectiveAt);
        var maxTime = points.Max(p => p.EffectiveAt);
        var existingTimes = (await _uow.CurrencyRateSpecs.ListBySpecAsync(
                new CurrencyRatesPairRangeSpec(from, to, minTime, maxTime)))
            .Select(r => r.EffectiveAt)
            .ToHashSet();

        // lastRate قبل از minTime برای sanity
        var lastBefore = await _uow.CurrencyRateSpecs.GetBySpecAsync(
            new LatestCurrencyRateSpec(from, to, minTime.AddTicks(-1)));
        decimal? lastRate = lastBefore?.Rate;

        var toInsert = new List<CurrencyRate>();
        int skipped = 0;
        var ordered = points.OrderBy(p => p.EffectiveAt);

        foreach (var p in ordered)
        {
            ct.ThrowIfCancellationRequested();

            if (p.Rate <= 0m) { skipped++; continue; }
            if (p.Rate < minRate || p.Rate > maxRate) { skipped++; continue; }

            if (lastRate.HasValue)
            {
                var changePct = Math.Abs((p.Rate - lastRate.Value) / lastRate.Value * 100m);
                if (changePct > maxDailyChangePct) { skipped++; continue; }
            }

            if (existingTimes.Contains(p.EffectiveAt)) { skipped++; continue; }

            toInsert.Add(CurrencyRate.Create(from, to, p.Rate, p.EffectiveAt, p.Source));
            lastRate = p.Rate;
        }

        if (toInsert.Count > 0)
        {
            await _uow.Context.CurrencyRates.AddRangeAsync(toInsert, ct);
            await _uow.SaveChangesAsync(ct);
        }

        if (isBackfill)
        {
            // Update progress — heuristic: pages fetched / estimated pages
            // (فقط برای UX؛ 100% توسط MarkCompleted در انتها)
            job.UpdateBackfillProgress(50m);
        }

        return (toInsert.Count, skipped);
    }

    private (decimal minRate, decimal maxRate, decimal maxDailyChangePct) ReadSanityBounds(
        PriceSource source, out TgjuConfigModel? config)
    {
        config = null;
        try
        {
            if (source.Type == PriceSourceType.Tgju)
            {
                config = JsonSerializer.Deserialize<TgjuConfigModel>(
                    source.ConfigJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch { /* ignore — defaults used */ }

        var minRate = config?.MinRate ?? _opts.DefaultMinRate;
        var maxRate = config?.MaxRate ?? _opts.DefaultMaxRate;
        var maxDailyChangePct = config?.MaxDailyChangePct ?? _opts.DefaultMaxDailyChangePct;

        return (minRate, maxRate, maxDailyChangePct);
    }

    private async Task<DateTime?> GetMaxEffectiveAtAsync(
    string from, string to, string sourceCode)
    {
        var latest = await _uow.CurrencyRateSpecs.GetBySpecAsync(
            new LatestCurrencyRateBySourceSpec(from, to, sourceCode, DateTime.UtcNow));
        return latest?.EffectiveAt;
    }

    /* ═══════════════════ Queries ═══════════════════ */

    // ─── IPriceSourceQueryService ───

    public async Task<IReadOnlyList<PriceSourceView>> GetListAsync(
        CancellationToken ct = default)
    {
        var list = await _uow.PriceSources.GetAllAsync(q => q.OrderBy(s => s.Code));
        return list.Select(MapSource).ToList();
    }

    async Task<PriceSourceView?> IPriceSourceQueryService.GetByIdAsync(
        Guid id, CancellationToken ct)
    {
        var s = await _uow.PriceSources.GetByIdAsync(id);
        return s is null ? null : MapSource(s);
    }

    // ─── IPriceCollectionJobQueryService ───

    public async Task<IReadOnlyList<PriceCollectionJobView>> GetListAsync(
        Guid? priceSourceId, int take = 100, CancellationToken ct = default)
    {
        var takeClamped = Math.Clamp(take, 1, 500);
        var jobs = await _uow.PriceCollectionJobs.GetAllAsync(q =>
        {
            IQueryable<PriceCollectionJob> query = q;
            if (priceSourceId.HasValue)
                query = query.Where(j => j.PriceSourceId == priceSourceId.Value);
            return query.OrderByDescending(j => j.CreatedAt).Take(takeClamped);
        });

        var sources = (await _uow.PriceSources.GetAllAsync())
            .ToDictionary(s => s.Id);

        return jobs.Select(j => MapJob(j, sources.GetValueOrDefault(j.PriceSourceId)))
            .ToList();
    }

    async Task<PriceCollectionJobView?> IPriceCollectionJobQueryService.GetByIdAsync(
        Guid id, CancellationToken ct)
    {
        var j = await _uow.PriceCollectionJobs.GetByIdAsync(id);
        if (j is null) return null;

        var s = await _uow.PriceSources.GetByIdAsync(j.PriceSourceId);
        return MapJob(j, s);
    }

    public Task SaveAsync() => _uow.SaveChangesAsync();
    /* ═══════════════════ Mappers ═══════════════════ */

    private static PriceSourceView MapSource(PriceSource s) => new(
        s.Id, s.Code, s.Name, (int)s.Type, s.Type.ToString(),
        s.ConfigJson, s.FromAsset, s.ToAsset, s.IsActive, s.CreatedAt);

    private static PriceCollectionJobView MapJob(
        PriceCollectionJob j, PriceSource? source) => new(
        j.Id, j.PriceSourceId,
        source?.Code ?? "—",
        source?.Name ?? "—",
        (int)j.Mode, j.Mode.ToString(),
        (int)j.State, j.State.ToString(),
        j.RecurrencePatternJson,
        j.SchedulerRecurringId, j.SchedulerJobId,
        j.StartedAt, j.StoppedAt, j.LastRunAt,
        j.LastRunInserted, j.LastRunSkipped, j.TotalInserted,
        j.BackfillProgressPercent, j.LastError, j.CreatedAt);
}

public sealed class PriceCollectionOptions
{
    public const string SectionName = "Trader:PriceCollection";

    public decimal DefaultMinRate { get; set; } = 100_000m;
    public decimal DefaultMaxRate { get; set; } = 50_000_000m;
    public decimal DefaultMaxDailyChangePct { get; set; } = 20m;
    public int RateLimitMsBetweenPages { get; set; } = 250;
}