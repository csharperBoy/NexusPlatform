using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;
using Scheduler.Domain.Entities;
using Scheduler.Domain.Enums;
using Scheduler.Domain.ValueObjects;
using Scheduler.Infrastructure.Data;

namespace Scheduler.Infrastructure.Jobs;

public class RecurringScheduleRecovery : IRecurringScheduleRecovery
{
    private readonly ISchedulerUnitOfWork<SchedulerDbContext> _uow;
    private readonly ISchedulerService _scheduler;
    private readonly IBackgroundJobClient _jobs;
    private readonly ILogger<RecurringScheduleRecovery> _logger;
    private readonly SchedulerOptions _options;
    public RecurringScheduleRecovery(
        ISchedulerUnitOfWork<SchedulerDbContext> uow,
        ISchedulerService scheduler,
        IBackgroundJobClient jobs,
        IOptions<SchedulerOptions> options,
        ILogger<RecurringScheduleRecovery> logger)
    {
        _uow = uow;
        _scheduler = scheduler;
        _jobs = jobs;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RecoverAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("RecurringSchedule recovery starting...");

        var all = await _uow.RecurringSchedules.GetAllAsync();
        var schedules = all.ToList();

        if (schedules.Count == 0)
        {
            _logger.LogInformation(
                "RecurringSchedule recovery: no schedules found.");
            return;
        }

        var active = 0;
        var scheduled = 0;
        var cancelled = 0;
        var completed = 0;

        foreach (var rec in schedules)
        {
            try
            {
                var outcome = await RecoverOneAsync(rec, ct);
                switch (outcome)
                {
                    case RecoveryOutcome.AlreadyScheduled:
                        active++;
                        break;
                    case RecoveryOutcome.Rescheduled:
                        scheduled++;
                        break;
                    case RecoveryOutcome.CancelledOrphan:
                        cancelled++;
                        break;
                    case RecoveryOutcome.MarkedCompleted:
                        completed++;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Recovery failed for RecurringSchedule {Id} '{Name}'.",
                    rec.Id, rec.Name);
            }
        }

        _logger.LogInformation(
            "RecurringSchedule recovery completed. " +
            "Total={Total} ActiveOk={Active} Rescheduled={Rescheduled} " +
            "OrphansCancelled={Cancelled} Completed={Completed}",
            schedules.Count, active, scheduled, cancelled, completed);
    }

    /* ═══════════════════ Core ═══════════════════ */

    private async Task<RecoveryOutcome> RecoverOneAsync(
        RecurringSchedule rec, CancellationToken ct)
    {
        var pendingJobs = (await _uow.ScheduledJobs.GetAllAsync(
            q => q.Where(s => s.RecurringScheduleId == rec.Id
                           && s.State == ScheduledJobState.Pending)))
            .ToList();

        switch (rec.State)
        {
            case RecurringScheduleState.Active:
                if (pendingJobs.Count > 0)
                {
                    // Already scheduled — healthy
                    return RecoveryOutcome.AlreadyScheduled;
                }
                // Orphan: schedule next
                return await RescheduleActiveAsync(rec, ct);

            case RecurringScheduleState.Paused:
            case RecurringScheduleState.Cancelled:
            case RecurringScheduleState.Completed:
                if (pendingJobs.Count > 0)
                {
                    await CancelOrphansAsync(pendingJobs, ct);
                    return RecoveryOutcome.CancelledOrphan;
                }
                return RecoveryOutcome.AlreadyScheduled;
        }

        return RecoveryOutcome.AlreadyScheduled;
    }

    private async Task<RecoveryOutcome> RescheduleActiveAsync(
        RecurringSchedule rec, CancellationToken ct)
    {
        // ─── Bounds check ───
        if (rec.MaxOccurrences.HasValue
            && rec.OccurredCount >= rec.MaxOccurrences.Value)
        {
            rec.Complete();
            await _uow.RecurringSchedules.UpdateAsync(rec);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Recovery: RecurringSchedule {Id} completed (max occurrences).",
                rec.Id);
            return RecoveryOutcome.MarkedCompleted;
        }

        var tz = TimeZoneInfo.FindSystemTimeZoneById(rec.TimeZoneId);
        var pattern = RecurrencePattern.FromJson(rec.RecurrencePatternJson);
        var now = DateTimeOffset.UtcNow;

        // ─── اگر NextRunAt در آینده است، از همان استفاده کن ───
        DateTimeOffset nextFireAt;
        if (rec.NextRunAt.HasValue && rec.NextRunAt.Value > now)
        {
            nextFireAt = rec.NextRunAt.Value;
        }
        else
        {
            nextFireAt = pattern.GetNextOccurrence(rec.StartAt, now, tz);
        }

        // ─── EndAt check ───
        if (rec.EndAt.HasValue && nextFireAt > rec.EndAt.Value)
        {
            rec.Complete();
            await _uow.RecurringSchedules.UpdateAsync(rec);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Recovery: RecurringSchedule {Id} completed (end date).",
                rec.Id);
            return RecoveryOutcome.MarkedCompleted;
        }

        // ─── Schedule via SchedulerService ───
        // اما ISchedulerService.ScheduleRecurringAsync Schedule جدید می‌سازد — نه
        // می‌خواهیم دوباره RecurringSchedule بسازیم. به‌جایش مستقیم Occurrence بساز.
        await ScheduleOccurrenceDirectlyAsync(rec, nextFireAt, ct);

        _logger.LogWarning(
            "Recovery: RecurringSchedule {Id} '{Name}' had no pending occurrence. " +
            "Rescheduled for {FireAt}.",
            rec.Id, rec.Name, nextFireAt);

        return RecoveryOutcome.Rescheduled;
    }

    private async Task ScheduleOccurrenceDirectlyAsync(
        RecurringSchedule rec, DateTimeOffset fireAt, CancellationToken ct)
    {
        // Pre-fire buffer از SchedulerOptions — اما IOptions در scope موجود است.
        // برای سادگی، همان 15s ثابت.
        //var preFire = TimeSpan.FromSeconds(15);
        var preFire = _options.DefaultPreFireBuffer;
        var triggerAt = fireAt - preFire;
        if (triggerAt <= DateTimeOffset.UtcNow)
            triggerAt = DateTimeOffset.UtcNow.AddSeconds(1);

        var newJob = ScheduledJob.Create(
            jobType: rec.JobType,
            hangfireJobId: "PENDING",
            payloadJson: rec.PayloadJson,
            fireAt: fireAt,
            preFireBuffer: preFire,
            recurringScheduleId: rec.Id);

        await _uow.ScheduledJobs.AddAsync(newJob);
        await _uow.SaveChangesAsync(ct);

        var hangfireJobId = _jobs.Schedule<HangfireJobDispatcher>(
            d => d.DispatchAsync(
                newJob.Id,
                rec.JobType,
                rec.PayloadJson,
                fireAt.ToUnixTimeMilliseconds(),
                CancellationToken.None),
            triggerAt);

        newJob.SetHangfireJobId(hangfireJobId);
        await _uow.ScheduledJobs.UpdateAsync(newJob);

        rec.MarkNextScheduled(newJob.Id, fireAt);
        await _uow.RecurringSchedules.UpdateAsync(rec);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task CancelOrphansAsync(
        List<ScheduledJob> orphans, CancellationToken ct)
    {
        foreach (var orphan in orphans)
        {
            try
            {
                if (!string.IsNullOrEmpty(orphan.HangfireJobId)
                    && orphan.HangfireJobId != "PENDING")
                {
                    _jobs.Delete(orphan.HangfireJobId);
                }

                orphan.MarkCancelled();
                await _uow.ScheduledJobs.UpdateAsync(orphan);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Recovery: failed to cancel orphan {Id}.", orphan.Id);
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    private enum RecoveryOutcome
    {
        AlreadyScheduled,
        Rescheduled,
        CancelledOrphan,
        MarkedCompleted,
    }
}