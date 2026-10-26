using Scheduler.Domain.Enums;

namespace Scheduler.Application.Models;

public record RecurringScheduleInfo(
    Guid Id,
    string Name,
    string JobType,
    RecurringScheduleState State,
    DateTimeOffset StartAt,
    DateTimeOffset? EndAt,
    int? MaxOccurrences,
    int OccurredCount,
    int FailureCount,
    DateTimeOffset? LastRunAt,
    DateTimeOffset? NextRunAt,
    string TimeZoneId);