using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.Entities;
using Scheduler.Domain.Enums;
using Scheduler.Infrastructure.Data;
using System.Text.Json;

namespace Scheduler.Infrastructure.Jobs;

public class HangfireJobDispatcher
{
    private readonly IServiceProvider _rootSp;
    private readonly JobTypeRegistry _registry;
    private readonly ILogger<HangfireJobDispatcher> _logger;

    public HangfireJobDispatcher(
        IServiceProvider rootSp,
        JobTypeRegistry registry,
        ILogger<HangfireJobDispatcher> logger)
    {
        _rootSp = rootSp;
        _registry = registry;
        _logger = logger;
    }

    [Queue("scheduler")]
    [AutomaticRetry(Attempts = 0)]
    public async Task DispatchAsync(
        Guid scheduledJobId,
        string jobTypeKey,
        string payloadJson,
        long fireAtUnixMs,
        CancellationToken ct)
    {
        using var scope = _rootSp.CreateScope();
        var sp = scope.ServiceProvider;
        var uow = sp.GetRequiredService<ISchedulerUnitOfWork<SchedulerDbContext>>();
        var job = await uow.ScheduledJobs.GetByIdAsync(scheduledJobId);

        // Idempotency
        if (job is null) return;
        if (job.State == ScheduledJobState.Succeeded
            || job.State == ScheduledJobState.Cancelled) return;

        try
        {
            job.MarkExecuting();
            await uow.ScheduledJobs.UpdateAsync(job);
            await uow.SaveChangesAsync(ct);

            var payloadType = _registry.Resolve(jobTypeKey)
                ?? throw new InvalidOperationException(
                    $"No payload type registered for key '{jobTypeKey}'");

            var payload = JsonSerializer.Deserialize(payloadJson, payloadType, SchedulerJson.Options)
                ?? throw new InvalidOperationException("Payload deserialization returned null.");

            await PreciseDelay.UntilAsync(fireAtUnixMs, ct);

            var handlerType = typeof(IScheduledJobHandler<>).MakeGenericType(payloadType);
            var handler = sp.GetService(handlerType)
                ?? throw new InvalidOperationException(
                    $"No handler registered for payload type {payloadType.FullName}");

            var method = handlerType.GetMethod(
                nameof(IScheduledJobHandler<IScheduledJobPayload>.ExecuteAsync))!;
            var task = (Task)method.Invoke(handler, [payload, ct])!;
            await task;

            job.MarkSucceeded();
            await uow.ScheduledJobs.UpdateAsync(job);
            await uow.SaveChangesAsync(ct);

            // Reschedule if recurring
            if (job.RecurringScheduleId is Guid recId)
            {
                try
                {
                    var rescheduler = sp.GetRequiredService<IRecurringScheduleRescheduler>();
                    await rescheduler.RescheduleNextAsync(
                        recId, job.Id, success: true, ct);
                }
                catch (Exception rEx)
                {
                    _logger.LogError(rEx,
                        "Failed to reschedule RecurringSchedule {RecId} after success of {JobId}.",
                        recId, job.Id);
                }
            }

            _logger.LogInformation(
                "Scheduled job {JobId} ({JobType}) executed successfully",
                scheduledJobId, jobTypeKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Scheduled job {JobId} ({JobType}) failed",
                scheduledJobId, jobTypeKey);

            job.MarkFailed(ex.Message);
            await uow.ScheduledJobs.UpdateAsync(job);
            await uow.SaveChangesAsync(CancellationToken.None);

            // Reschedule even on failure
            if (job.RecurringScheduleId is Guid recId)
            {
                try
                {
                    var rescheduler = sp.GetRequiredService<IRecurringScheduleRescheduler>();
                    await rescheduler.RescheduleNextAsync(
                        recId, job.Id, success: false, CancellationToken.None);
                }
                catch (Exception rEx)
                {
                    _logger.LogError(rEx,
                        "Failed to reschedule RecurringSchedule {RecId} after failure of {JobId}.",
                        recId, job.Id);
                }
            }

            throw;
        }
    }
}