using Scheduler.Application.Abstractions;
using Scheduler.Application.Attributes;

namespace Trader.Application.Scheduler;

/// <summary>
/// Payload برای Job روزانه‌ی پاک‌سازی رویدادهای قدیمی.
/// Key در Scheduler Registry با این ثبت می‌شود.
/// </summary>
[ScheduledJob(Key)]
public sealed record RetentionCleanupPayload : IScheduledJobPayload
{
    public const string Key = "trader.event-retention";

    public int RetentionDays { get; init; } = 90;
}