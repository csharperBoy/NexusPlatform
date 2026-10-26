using Core.Domain.Common.EntityProperties;
using Scheduler.Domain.Enums;

namespace Scheduler.Domain.Entities;

public class RecurringSchedule : BaseEntity, IAuditableEntity, IOwnerableEntity
{
    public string Name { get; private set; } = default!;
    public string JobType { get; private set; } = default!;
    public string PayloadJson { get; private set; } = default!;

    public string RecurrencePatternJson { get; private set; } = default!;
    public string TimeZoneId { get; private set; } = default!;

    public DateTimeOffset StartAt { get; private set; }
    public DateTimeOffset? EndAt { get; private set; }
    public int? MaxOccurrences { get; private set; }
    public int MaxFailures { get; private set; } = 10;

    public RecurringScheduleState State { get; private set; }
    public int OccurredCount { get; private set; }
    public int FailureCount { get; private set; }
    public DateTimeOffset? LastRunAt { get; private set; }
    public DateTimeOffset? NextRunAt { get; private set; }
    public Guid? CurrentScheduledJobId { get; private set; }

    // IOwnerableEntity
    public Guid? OwnerOrganizationUnitId { get; protected set; }
    public Guid? OwnerPositionId { get; protected set; }
    public Guid? OwnerPersonId { get; protected set; }
    public Guid? OwnerUserId { get; protected set; }

    public void SetOwners(Guid? userId, Guid? personId, Guid? positionId, Guid? orgUnitId)
    {
        OwnerUserId = userId;
        OwnerPersonId = personId;
        OwnerPositionId = positionId;
        OwnerOrganizationUnitId = orgUnitId;
    }
    public void SetPersonOwner(Guid personId) => OwnerPersonId = personId;
    public void SetUserOwner(Guid userId) => OwnerUserId = userId;
    public void SetPositionOwner(Guid positionId) => OwnerPositionId = positionId;
    public void SetOrganizationUnitOwner(Guid orgUnitId) => OwnerOrganizationUnitId = orgUnitId;

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private RecurringSchedule() { } // EF

    public static RecurringSchedule Create(
        string name,
        string jobType,
        string payloadJson,
        string recurrencePatternJson,
        string timeZoneId,
        DateTimeOffset startAt,
        DateTimeOffset? endAt,
        int? maxOccurrences,
        int maxFailures = 10)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(jobType))
            throw new ArgumentException("JobType is required.", nameof(jobType));
        if (string.IsNullOrWhiteSpace(recurrencePatternJson))
            throw new ArgumentException("RecurrencePattern is required.", nameof(recurrencePatternJson));
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new ArgumentException("TimeZoneId is required.", nameof(timeZoneId));
        if (endAt.HasValue && startAt >= endAt.Value)
            throw new InvalidOperationException("StartAt must be before EndAt.");
        if (maxOccurrences.HasValue && maxOccurrences.Value <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxOccurrences), "Must be > 0.");
        if (maxFailures <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxFailures), "Must be > 0.");

        return new RecurringSchedule
        {
            Name = name.Trim(),
            JobType = jobType,
            PayloadJson = payloadJson,
            RecurrencePatternJson = recurrencePatternJson,
            TimeZoneId = timeZoneId,
            StartAt = startAt,
            EndAt = endAt,
            MaxOccurrences = maxOccurrences,
            MaxFailures = maxFailures,
            State = RecurringScheduleState.Active,
            OccurredCount = 0,
            FailureCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkNextScheduled(Guid scheduledJobId, DateTimeOffset nextRunAt)
    {
        CurrentScheduledJobId = scheduledJobId;
        NextRunAt = nextRunAt;
        Touch();
    }

    public void MarkExecutionCompleted(DateTimeOffset now)
    {
        OccurredCount++;
        LastRunAt = now;
        FailureCount = 0;
        Touch();
    }

    public void MarkExecutionFailed(DateTimeOffset now)
    {
        OccurredCount++;
        FailureCount++;
        LastRunAt = now;
        if (FailureCount >= MaxFailures)
            State = RecurringScheduleState.Paused;
        Touch();
    }

    public void Pause()
    {
        if (State == RecurringScheduleState.Active) { State = RecurringScheduleState.Paused; Touch(); }
    }

    public void Resume()
    {
        if (State == RecurringScheduleState.Paused) { State = RecurringScheduleState.Active; FailureCount = 0; Touch(); }
    }

    public void Complete() { State = RecurringScheduleState.Completed; Touch(); }
    public void Cancel() { State = RecurringScheduleState.Cancelled; Touch(); }
}