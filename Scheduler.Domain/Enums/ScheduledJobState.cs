namespace Scheduler.Domain.Enums
{
    public enum ScheduledJobState
    {
        Pending = 0,
        Executing = 1,
        Succeeded = 2,
        Failed = 3,
        Cancelled = 4,
    }
}
