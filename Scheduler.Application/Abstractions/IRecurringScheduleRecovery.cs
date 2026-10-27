namespace Scheduler.Application.Abstractions;

/// <summary>
/// در Startup، RecurringScheduleهای Active که Occurrence Pending ندارند را
/// بازسازی می‌کند. همچنین Orphan Jobهای Paused/Cancelled را پاک می‌کند.
/// </summary>
public interface IRecurringScheduleRecovery
{
    Task RecoverAsync(CancellationToken ct = default);
}