namespace Scheduler.Application.Abstractions;

public interface IRecurringScheduleRescheduler
{
    Task RescheduleNextAsync(
        Guid recurringScheduleId,
        Guid currentScheduledJobId,
        bool success,
        CancellationToken ct = default);
}