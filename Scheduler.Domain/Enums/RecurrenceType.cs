namespace Scheduler.Domain.Enums;

/// <summary>
/// انواع Recurrence. مقادیر Append می‌شوند (نه Insert) تا JSON ردیف‌های
/// موجود در DB بدون Migration معتبر بمانند.
/// </summary>
public enum RecurrenceType
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Minutely = 3,
    Hourly = 4,
    Cron = 5,
}