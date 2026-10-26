using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;
using Scheduler.Domain.Entities;
using Scheduler.Domain.Enums;
using Scheduler.Domain.ValueObjects;
using Scheduler.Infrastructure.Data;

namespace Scheduler.Infrastructure.Jobs;

public class RecurringScheduleRescheduler : IRecurringScheduleRescheduler
{
    private readonly ISchedulerUnitOfWork<SchedulerDbContext> _uow;
    private readonly IBackgroundJobClient _jobs;
    private readonly SchedulerOptions _options;
    private readonly ILogger<RecurringScheduleRescheduler> _logger;

    public RecurringScheduleRescheduler(
        ISchedulerUnitOfWork<SchedulerDbContext> uow,
        IBackgroundJobClient jobs,
        IOptions<SchedulerOptions> options,
        ILogger<RecurringScheduleRescheduler> logger)
    {
        _uow = uow;
        _jobs = jobs;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RescheduleNextAsync(
        Guid recurringScheduleId,
        Guid currentScheduledJobId,
        bool success,
        CancellationToken ct = default)
    {
        var recurring = await _uow.RecurringSchedules.GetByIdAsync(recurringScheduleId);
        if (recurring is null)
        {
            _logger.LogWarning(
                "RecurringSchedule {Id} not found for reschedule.", recurringScheduleId);
            return;
        }

        // Idempotency
        if (recurring.CurrentScheduledJobId != currentScheduledJobId)
        {
            _logger.LogDebug(
                "RecurringSchedule {Id} already advanced. Skipping.",
                recurringScheduleId);
            return;
        }

        var now = DateTimeOffset.UtcNow;

        if (success)
            recurring.MarkExecutionCompleted(now);
        else
            recurring.MarkExecutionFailed(now);

        if (recurring.State == RecurringScheduleState.Paused)
        {
            _logger.LogWarning(
                "RecurringSchedule {Id} '{Name}' paused after {Fails} failures.",
                recurring.Id, recurring.Name, recurring.FailureCount);
            await _uow.SaveChangesAsync(ct);
            return;
        }

        if (recurring.MaxOccurrences.HasValue
            && recurring.OccurredCount >= recurring.MaxOccurrences.Value)
        {
            recurring.Complete();
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "RecurringSchedule {Id} '{Name}' completed ({Count} occurrences).",
                recurring.Id, recurring.Name, recurring.OccurredCount);
            return;
        }

        var tz = TimeZoneInfo.FindSystemTimeZoneById(recurring.TimeZoneId);
        var pattern = RecurrencePattern.FromJson(recurring.RecurrencePatternJson);
        var nextFireAt = pattern.GetNextOccurrence(recurring.StartAt, now, tz);

        if (recurring.EndAt.HasValue && nextFireAt > recurring.EndAt.Value)
        {
            recurring.Complete();
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "RecurringSchedule {Id} '{Name}' completed (end date reached).",
                recurring.Id, recurring.Name);
            return;
        }

        var preFire = _options.DefaultPreFireBuffer;
        var triggerAt = nextFireAt - preFire;
        if (triggerAt <= DateTimeOffset.UtcNow)
            triggerAt = DateTimeOffset.UtcNow.AddSeconds(1);

        var nextScheduled = ScheduledJob.Create(
            jobType: recurring.JobType,
            hangfireJobId: "PENDING",
            payloadJson: recurring.PayloadJson,
            fireAt: nextFireAt,
            preFireBuffer: preFire,
            recurringScheduleId: recurring.Id);

        await _uow.ScheduledJobs.AddAsync(nextScheduled);
        await _uow.SaveChangesAsync(ct);

        var hangfireJobId = _jobs.Schedule<HangfireJobDispatcher>(
            d => d.DispatchAsync(
                nextScheduled.Id,
                recurring.JobType,
                recurring.PayloadJson,
                nextFireAt.ToUnixTimeMilliseconds(),
                CancellationToken.None),
            triggerAt);

        nextScheduled.SetHangfireJobId(hangfireJobId);
        await _uow.ScheduledJobs.UpdateAsync(nextScheduled);

        recurring.MarkNextScheduled(nextScheduled.Id, nextFireAt);
        await _uow.RecurringSchedules.UpdateAsync(recurring);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "RecurringSchedule {Id} '{Name}' → next {FireAt}",
            recurring.Id, recurring.Name, nextFireAt);
    }
}