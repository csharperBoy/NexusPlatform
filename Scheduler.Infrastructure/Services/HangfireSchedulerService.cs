using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;
using Scheduler.Application.Queries;
using Scheduler.Domain.Entities;
using Scheduler.Domain.Enums;
using Scheduler.Domain.ValueObjects;
using Scheduler.Infrastructure.Data;
using Scheduler.Infrastructure.Jobs;
using System.Text.Json;

namespace Scheduler.Infrastructure.Services;

class HangfireSchedulerService : ISchedulerService
{
    private readonly IBackgroundJobClient _jobs;
    private readonly JobTypeRegistry _registry;
    private readonly ISchedulerUnitOfWork<SchedulerDbContext> _uow;
    private readonly SchedulerOptions _options;
    private readonly ILogger<HangfireSchedulerService> _logger;

    public HangfireSchedulerService(
        IBackgroundJobClient jobs,
        JobTypeRegistry registry,
        ISchedulerUnitOfWork<SchedulerDbContext> uow,
        IOptions<SchedulerOptions> options,
        ILogger<HangfireSchedulerService> logger)
    {
        _jobs = jobs;
        _registry = registry;
        _uow = uow;
        _options = options.Value;
        _logger = logger;
    }

    /* ═══════════════════ One-Off ═══════════════════ */

    public async Task<Guid> ScheduleAsync<TPayload>(
        TPayload payload,
        DateTimeOffset fireAt,
        string? queue = null,
        TimeSpan? preFireBuffer = null,
        CancellationToken ct = default)
        where TPayload : IScheduledJobPayload
    {
        var jobTypeKey = _registry.GetKey(typeof(TPayload));
        var preFire = preFireBuffer ?? _options.DefaultPreFireBuffer;
        var triggerAt = fireAt - preFire;
        if (triggerAt <= DateTimeOffset.UtcNow)
            triggerAt = DateTimeOffset.UtcNow.AddSeconds(1);

        var payloadJson = JsonSerializer.Serialize(payload, SchedulerJson.Options);

        var domainJob = ScheduledJob.Create(
            jobType: jobTypeKey,
            hangfireJobId: "PENDING",
            payloadJson: payloadJson,
            fireAt: fireAt,
            preFireBuffer: preFire);

        await _uow.ScheduledJobs.AddAsync(domainJob);
        await _uow.SaveChangesAsync(ct);

        var hangfireJobId = _jobs.Schedule<HangfireJobDispatcher>(
            d => d.DispatchAsync(
                domainJob.Id,
                jobTypeKey,
                payloadJson,
                fireAt.ToUnixTimeMilliseconds(),
                CancellationToken.None),
            triggerAt);

        domainJob.SetHangfireJobId(hangfireJobId);
        await _uow.ScheduledJobs.UpdateAsync(domainJob);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Scheduled job {JobId} type={Type} fireAt={FireAt} triggerAt={TriggerAt}",
            domainJob.Id, jobTypeKey, fireAt, triggerAt);

        return domainJob.Id;
    }

    public async Task<bool> CancelAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _uow.ScheduledJobs.GetByIdAsync(jobId);
        if (job is null || job.State != ScheduledJobState.Pending)
            return false;

        var cancelled = _jobs.Delete(job.HangfireJobId);
        if (cancelled)
        {
            job.MarkCancelled();
            await _uow.ScheduledJobs.UpdateAsync(job);
            await _uow.SaveChangesAsync(ct);
        }
        return cancelled;
    }

    public async Task<ScheduledJobInfo?> GetAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _uow.ScheduledJobs.GetByIdAsync(jobId);
        return job is null ? null : ToInfo(job);
    }

    public async Task<IReadOnlyList<ScheduledJobInfo>> QueryAsync(
        SchedulerQuery query, CancellationToken ct = default)
    {
        var all = await _uow.ScheduledJobs.GetAllAsync();
        var q = all.AsQueryable();

        if (query.JobType is not null)
            q = q.Where(x => x.JobType == query.JobType);
        if (query.State is not null)
            q = q.Where(x => x.State == query.State);
        if (query.FireAtFrom is not null)
            q = q.Where(x => x.FireAt >= query.FireAtFrom);
        if (query.FireAtTo is not null)
            q = q.Where(x => x.FireAt <= query.FireAtTo);

        return q.OrderBy(x => x.FireAt)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(ToInfo)
            .ToList();
    }

    /* ═══════════════════ Recurring ═══════════════════ */

    public async Task<Guid> ScheduleRecurringAsync<TPayload>(
        string name,
        TPayload payload,
        RecurrencePattern pattern,
        DateTimeOffset? startAt = null,
        DateTimeOffset? endAt = null,
        int? maxOccurrences = null,
        int? maxFailures = 10,
        TimeZoneInfo? timeZone = null,
        string? queue = null,
        CancellationToken ct = default)
        where TPayload : IScheduledJobPayload
    {
        var tz = timeZone ?? TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");
        var start = startAt ?? DateTimeOffset.UtcNow;
        var jobTypeKey = _registry.GetKey(typeof(TPayload));
        var payloadJson = JsonSerializer.Serialize(payload, SchedulerJson.Options);
        var patternJson = pattern.ToJson();

        var existing = (await _uow.RecurringSchedules.GetAllAsync())
            .FirstOrDefault(r => r.Name == name);
        if (existing is not null)
            throw new InvalidOperationException(
                $"A RecurringSchedule named '{name}' already exists (Id: {existing.Id}).");

        var recurring = RecurringSchedule.Create(
            name: name,
            jobType: jobTypeKey,
            payloadJson: payloadJson,
            recurrencePatternJson: patternJson,
            timeZoneId: tz.Id,
            startAt: start,
            endAt: endAt,
            maxOccurrences: maxOccurrences,
            maxFailures: maxFailures ?? 10);

        await _uow.RecurringSchedules.AddAsync(recurring);
        await _uow.SaveChangesAsync(ct);

        await ScheduleOccurrenceAsync(recurring, start, ct);

        _logger.LogInformation(
            "RecurringSchedule {Id} '{Name}' created. JobType={Type} FirstFireAt={FireAt}",
            recurring.Id, recurring.Name, jobTypeKey, start);

        return recurring.Id;
    }

    public async Task<RecurringScheduleInfo?> GetRecurringAsync(
        Guid recurringScheduleId, CancellationToken ct = default)
    {
        var recurring = await _uow.RecurringSchedules.GetByIdAsync(recurringScheduleId);
        return recurring is null ? null : ToRecurringInfo(recurring);
    }

    public async Task<IReadOnlyList<RecurringScheduleInfo>> QueryRecurringAsync(
        RecurringSchedulerQuery query, CancellationToken ct = default)
    {
        var all = await _uow.RecurringSchedules.GetAllAsync();
        var q = all.AsQueryable();

        if (query.JobType is not null)
            q = q.Where(x => x.JobType == query.JobType);
        if (query.State is not null)
            q = q.Where(x => x.State == query.State);

        return q.OrderBy(x => x.StartAt)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(ToRecurringInfo)
            .ToList();
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private async Task ScheduleOccurrenceAsync(
        RecurringSchedule recurring,
        DateTimeOffset fireAt,
        CancellationToken ct)
    {
        var preFire = _options.DefaultPreFireBuffer;
        var triggerAt = fireAt - preFire;
        if (triggerAt <= DateTimeOffset.UtcNow)
            triggerAt = DateTimeOffset.UtcNow.AddSeconds(1);

        var scheduledJob = ScheduledJob.Create(
            jobType: recurring.JobType,
            hangfireJobId: "PENDING",
            payloadJson: recurring.PayloadJson,
            fireAt: fireAt,
            preFireBuffer: preFire,
            recurringScheduleId: recurring.Id);

        await _uow.ScheduledJobs.AddAsync(scheduledJob);
        await _uow.SaveChangesAsync(ct);

        var hangfireJobId = _jobs.Schedule<HangfireJobDispatcher>(
            d => d.DispatchAsync(
                scheduledJob.Id,
                recurring.JobType,
                recurring.PayloadJson,
                fireAt.ToUnixTimeMilliseconds(),
                CancellationToken.None),
            triggerAt);

        scheduledJob.SetHangfireJobId(hangfireJobId);
        await _uow.ScheduledJobs.UpdateAsync(scheduledJob);

        recurring.MarkNextScheduled(scheduledJob.Id, fireAt);
        await _uow.RecurringSchedules.UpdateAsync(recurring);
        await _uow.SaveChangesAsync(ct);
    }

    private static ScheduledJobInfo ToInfo(ScheduledJob j) => new(
        j.Id, j.JobType, j.State, j.FireAt, j.StartedAt, j.CompletedAt, j.Error);

    private static RecurringScheduleInfo ToRecurringInfo(RecurringSchedule r) => new(
        r.Id, r.Name, r.JobType, r.State, r.StartAt, r.EndAt,
        r.MaxOccurrences, r.OccurredCount, r.FailureCount,
        r.LastRunAt, r.NextRunAt, r.TimeZoneId);

    /* ═══════════════════ Lifecycle ═══════════════════ */

    public async Task<bool> PauseRecurringAsync(
        Guid recurringScheduleId, CancellationToken ct = default)
    {
        var recurring = await _uow.RecurringSchedules.GetByIdAsync(recurringScheduleId);
        if (recurring is null || recurring.State != RecurringScheduleState.Active)
            return false;

        recurring.Pause();
        await _uow.RecurringSchedules.UpdateAsync(recurring);

        // Cancel pending Hangfire job
        await CancelPendingOccurrenceAsync(recurringScheduleId, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "RecurringSchedule {Id} '{Name}' paused.", recurring.Id, recurring.Name);
        return true;
    }

    public async Task<bool> ResumeRecurringAsync(
        Guid recurringScheduleId, CancellationToken ct = default)
    {
        var recurring = await _uow.RecurringSchedules.GetByIdAsync(recurringScheduleId);
        if (recurring is null || recurring.State != RecurringScheduleState.Paused)
            return false;

        // ─── Bounds check first ───
        if (recurring.MaxOccurrences.HasValue
            && recurring.OccurredCount >= recurring.MaxOccurrences.Value)
        {
            recurring.Complete();
            await _uow.RecurringSchedules.UpdateAsync(recurring);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "RecurringSchedule {Id} completed on Resume (max occurrences reached).",
                recurring.Id);
            return false;
        }

        var tz = TimeZoneInfo.FindSystemTimeZoneById(recurring.TimeZoneId);
        var pattern = RecurrencePattern.FromJson(recurring.RecurrencePatternJson);
        var now = DateTimeOffset.UtcNow;
        var nextFireAt = pattern.GetNextOccurrence(recurring.StartAt, now, tz);

        if (recurring.EndAt.HasValue && nextFireAt > recurring.EndAt.Value)
        {
            recurring.Complete();
            await _uow.RecurringSchedules.UpdateAsync(recurring);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "RecurringSchedule {Id} completed on Resume (end date reached).",
                recurring.Id);
            return false;
        }

        recurring.Resume();
        await _uow.RecurringSchedules.UpdateAsync(recurring);
        await _uow.SaveChangesAsync(ct);

        await ScheduleOccurrenceAsync(recurring, nextFireAt, ct);

        _logger.LogInformation(
            "RecurringSchedule {Id} '{Name}' resumed. Next {FireAt}",
            recurring.Id, recurring.Name, nextFireAt);
        return true;
    }

    public async Task<bool> CancelRecurringAsync(
        Guid recurringScheduleId, CancellationToken ct = default)
    {
        var recurring = await _uow.RecurringSchedules.GetByIdAsync(recurringScheduleId);
        if (recurring is null) return false;
        if (recurring.State == RecurringScheduleState.Cancelled
            || recurring.State == RecurringScheduleState.Completed)
            return false;

        recurring.Cancel();
        await _uow.RecurringSchedules.UpdateAsync(recurring);

        await CancelPendingOccurrenceAsync(recurringScheduleId, ct);

        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "RecurringSchedule {Id} '{Name}' cancelled.", recurring.Id, recurring.Name);
        return true;
    }

    /* ═══════════════════ Helper ═══════════════════ */

    private async Task CancelPendingOccurrenceAsync(
        Guid recurringScheduleId, CancellationToken ct)
    {
        var pendingJobs = await _uow.ScheduledJobs.GetAllAsync(
            q => q.Where(s => s.RecurringScheduleId == recurringScheduleId
                           && s.State == ScheduledJobState.Pending));

        foreach (var pending in pendingJobs)
        {
            try
            {
                if (!string.IsNullOrEmpty(pending.HangfireJobId)
                    && pending.HangfireJobId != "PENDING")
                {
                    _jobs.Delete(pending.HangfireJobId);
                }

                pending.MarkCancelled();
                await _uow.ScheduledJobs.UpdateAsync(pending);

                _logger.LogDebug(
                    "Cancelled pending ScheduledJob {Id} for RecurringSchedule {RecId}.",
                    pending.Id, recurringScheduleId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to cancel pending job {Id}.", pending.Id);
            }
        }
    }
}