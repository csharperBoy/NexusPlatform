using System.Text.Json;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.ValueObjects;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Dtos.HistoricalData;
using Trader.Application.Scheduler;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.HistoricalData;

public sealed class HistoricalDataCollectionService
    : IHistoricalDataCollectionCommandService,
      IHistoricalDataCollectionQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<HistoricalDataCollectionService> _logger;

    private static readonly TimeZoneInfo TehranTz =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

    public HistoricalDataCollectionService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ISchedulerService scheduler,
        ILogger<HistoricalDataCollectionService> logger)
    {
        _uow = uow;
        _scheduler = scheduler;
        _logger = logger;
    }

    /* ═══════════════ Commands ═══════════════ */

    public async Task<Guid> CreateBackfillAsync(
        CreateHistoricalBackfillModel model, CancellationToken ct = default)
    {
        var (isins, tfs) = Normalize(model.Isins, model.TimeFrames);

        if (model.DateTo <= model.DateFrom)
            throw new InvalidOperationException(
                "DateTo must be after DateFrom.");

        var job = HistoricalDataCollectionJob.CreateBackfill(
            name: model.Name,
            isinsJson: JsonSerializer.Serialize(isins),
            timeFramesJson: JsonSerializer.Serialize(tfs),
            dateFrom: model.DateFrom,
            dateTo: model.DateTo,
            totalCombinations: isins.Count * tfs.Count);

        await _uow.HistoricalDataCollectionJobs.AddAsync(job);

        _logger.LogInformation(
            "Backfill job created: {Id} name={Name} isins={I} tfs={T}",
            job.Id, job.Name, isins.Count, tfs.Count);

        return job.Id;
    }

    public async Task<Guid> CreateRecurringAsync(
        CreateHistoricalRecurringModel model, CancellationToken ct = default)
    {
        var (isins, tfs) = Normalize(model.Isins, model.TimeFrames);

        // Validate recurrence pattern
        _ = RecurrencePattern.FromJson(model.RecurrencePatternJson);

        var job = HistoricalDataCollectionJob.CreateRecurring(
            name: model.Name,
            isinsJson: JsonSerializer.Serialize(isins),
            timeFramesJson: JsonSerializer.Serialize(tfs),
            dateFrom: model.DateFrom,
            recurrencePatternJson: model.RecurrencePatternJson,
            totalCombinations: isins.Count * tfs.Count);

        await _uow.HistoricalDataCollectionJobs.AddAsync(job);

        _logger.LogInformation(
            "Recurring job created: {Id} name={Name} isins={I} tfs={T}",
            job.Id, job.Name, isins.Count, tfs.Count);

        return job.Id;
    }

    public async Task StartJobAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _uow.HistoricalDataCollectionJobs.GetByIdAsync(id)
            ?? throw new InvalidOperationException(
                $"HistoricalDataCollectionJob {id} not found.");

        if (job.State == HistoricalJobState.Running)
            throw new InvalidOperationException("Job is already Running.");

        if (job.Mode == HistoricalCollectionMode.Recurring)
        {
            var pattern = RecurrencePattern.FromJson(job.RecurrencePatternJson!);

            var schedulerId = await _scheduler.ScheduleRecurringAsync(
                name: $"hist-collect-{job.Id}",
                payload: new CollectCandlePayload(job.Id),
                pattern: pattern,
                startAt: DateTimeOffset.UtcNow.AddMinutes(1),
                endAt: null,
                maxOccurrences: null,
                maxFailures: 10,
                timeZone: TehranTz,
                queue: "scheduler");

            job.MarkRunningRecurring(schedulerId, DateTime.UtcNow);

            _logger.LogInformation(
                "HistoricalJob {Id} started (Recurring). SchedulerRecurringId={RecId}",
                job.Id, schedulerId);
        }
        else
        {
            var schedulerJobId = await _scheduler.ScheduleAsync(
                payload: new CollectCandlePayload(job.Id),
                fireAt: DateTimeOffset.UtcNow,
                queue: "scheduler");

            job.MarkRunningBackfill(schedulerJobId, DateTime.UtcNow);

            _logger.LogInformation(
                "HistoricalJob {Id} started (Backfill). SchedulerJobId={JobId}",
                job.Id, schedulerJobId);
        }

        await _uow.HistoricalDataCollectionJobs.UpdateAsync(job);
    }

    public async Task StopJobAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _uow.HistoricalDataCollectionJobs.GetByIdAsync(id)
            ?? throw new InvalidOperationException(
                $"HistoricalDataCollectionJob {id} not found.");

        if (job.State != HistoricalJobState.Running)
            return; // no-op

        if (job.Mode == HistoricalCollectionMode.Recurring
            && job.SchedulerRecurringId.HasValue)
        {
            try
            {
                await _scheduler.CancelRecurringAsync(job.SchedulerRecurringId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to cancel SchedulerRecurring {RecId}. DB will still be Stopped.",
                    job.SchedulerRecurringId.Value);
            }
        }
        else if (job.Mode == HistoricalCollectionMode.Backfill
            && job.SchedulerJobId.HasValue)
        {
            try
            {
                await _scheduler.CancelAsync(job.SchedulerJobId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to cancel SchedulerJob {JobId}. DB will still be Stopped.",
                    job.SchedulerJobId.Value);
            }
        }

        job.MarkStopped(DateTime.UtcNow);
        await _uow.HistoricalDataCollectionJobs.UpdateAsync(job);

        _logger.LogInformation("HistoricalJob {Id} stopped.", job.Id);
    }

    public Task SaveAsync(CancellationToken ct = default)
        => _uow.SaveChangesAsync(ct);

    /* ═══════════════ Queries ═══════════════ */

    public async Task<IReadOnlyList<HistoricalDataCollectionJobSummaryView>> GetListAsync(
        int take = 50, CancellationToken ct = default)
    {
        var takeClamped = Math.Clamp(take, 1, 500);
        var jobs = await _uow.HistoricalDataCollectionJobs.GetAllAsync(q =>
            q.OrderByDescending(j => j.CreatedAt).Take(takeClamped));

        return jobs.Select(MapToSummary).ToList();
    }

    public async Task<HistoricalDataCollectionJobDetailView?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var job = await _uow.HistoricalDataCollectionJobs.GetByIdAsync(id);
        return job is null ? null : MapToDetail(job);
    }

    /* ═══════════════ Helpers ═══════════════ */

    private static (List<string> Isins, List<int> Tfs) Normalize(
        IReadOnlyList<string> isins, IReadOnlyList<int> timeFrames)
    {
        if (isins is null || isins.Count == 0)
            throw new InvalidOperationException("حداقل یک نماد الزامی است.");

        if (timeFrames is null || timeFrames.Count == 0)
            throw new InvalidOperationException("حداقل یک تایم‌فریم الزامی است.");

        var distinctIsins = isins
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var distinctTfs = timeFrames.Distinct().ToList();

        if (distinctIsins.Count == 0)
            throw new InvalidOperationException("هیچ ISIN معتبری وجود ندارد.");

        return (distinctIsins, distinctTfs);
    }

    private static HistoricalDataCollectionJobSummaryView MapToSummary(
        HistoricalDataCollectionJob j) => new(
        j.Id, j.Name,
        (int)j.Mode, j.Mode.ToString(),
        (int)j.State, j.State.ToString(),
        j.SchedulerRecurringId, j.SchedulerJobId,
        j.RecurrencePatternJson,
        j.TotalCombinations, j.CompletedCombinations, j.TotalCandlesCollected,
        j.BackfillProgressPercent,
        j.CurrentIsin, j.CurrentTimeFrame,
        j.StartedAt, j.StoppedAt, j.LastRunAt,
        j.LastRunInserted, j.LastRunSkipped,
        j.CompletedAt, j.DurationMs,
        j.LastError, j.CreatedAt);

    private static HistoricalDataCollectionJobDetailView MapToDetail(
        HistoricalDataCollectionJob j)
    {
        List<string> isins;
        List<int> tfs;

        try { isins = JsonSerializer.Deserialize<List<string>>(j.IsinsJson) ?? new(); }
        catch { isins = new(); }

        try { tfs = JsonSerializer.Deserialize<List<int>>(j.TimeFramesJson) ?? new(); }
        catch { tfs = new(); }

        return new HistoricalDataCollectionJobDetailView(
            j.Id, j.Name,
            (int)j.Mode, j.Mode.ToString(),
            (int)j.State, j.State.ToString(),
            j.SchedulerRecurringId, j.SchedulerJobId,
            j.RecurrencePatternJson,
            isins, tfs,
            j.DateFrom, j.DateTo,
            j.TotalCombinations, j.CompletedCombinations, j.TotalCandlesCollected,
            j.BackfillProgressPercent,
            j.CurrentIsin, j.CurrentTimeFrame,
            j.StartedAt, j.StoppedAt, j.LastRunAt,
            j.LastRunInserted, j.LastRunSkipped,
            j.CompletedAt, j.DurationMs,
            j.LastError, j.CreatedAt);
    }
}