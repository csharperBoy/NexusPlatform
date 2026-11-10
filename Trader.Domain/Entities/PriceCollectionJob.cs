using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

/// <summary>
/// Job جمع‌آوری نرخ از یک PriceSource (D35).
/// Lifecycle مشابه TradingRun: Create → Start → Running ↔ Stopped (Recurring) / Completed (Backfill).
///
/// SchedulerRecurringId فقط برای Mode=Recurring پر می‌شود (D43).
/// SchedulerJobId فقط برای Mode=Backfill.
/// </summary>
public class PriceCollectionJob : BaseEntity, IAuditableEntity
{
    public Guid PriceSourceId { get; private set; }
    public PriceCollectionMode Mode { get; private set; }

    /// <summary>JSON RecurrencePattern — فقط برای Mode=Recurring. Backfill null.</summary>
    public string? RecurrencePatternJson { get; private set; }

    public PriceCollectionJobState State { get; private set; }

    /// <summary>Guid بازگشتی از ISchedulerService.ScheduleRecurringAsync. null = non-recurring.</summary>
    public Guid? SchedulerRecurringId { get; private set; }

    /// <summary>Guid بازگشتی از ISchedulerService.ScheduleAsync. null = non-backfill.</summary>
    public Guid? SchedulerJobId { get; private set; }

    public DateTime? StartedAt { get; private set; }
    public DateTime? StoppedAt { get; private set; }
    public DateTime? LastRunAt { get; private set; }

    public int LastRunInserted { get; private set; }
    public int LastRunSkipped { get; private set; }
    public int TotalInserted { get; private set; }

    /// <summary>درصد پیشرفت Backfill (0-100). Recurring null.</summary>
    public decimal? BackfillProgressPercent { get; private set; }

    public string? LastError { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private PriceCollectionJob() { } // EF

    public static PriceCollectionJob CreateRecurring(
        Guid priceSourceId,
        string recurrencePatternJson)
    {
        if (priceSourceId == Guid.Empty)
            throw new ArgumentException("PriceSourceId required.", nameof(priceSourceId));
        if (string.IsNullOrWhiteSpace(recurrencePatternJson))
            throw new ArgumentException("RecurrencePatternJson required.", nameof(recurrencePatternJson));

        return new PriceCollectionJob
        {
            PriceSourceId = priceSourceId,
            Mode = PriceCollectionMode.Recurring,
            RecurrencePatternJson = recurrencePatternJson.Trim(),
            State = PriceCollectionJobState.Created,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public static PriceCollectionJob CreateBackfill(Guid priceSourceId)
    {
        if (priceSourceId == Guid.Empty)
            throw new ArgumentException("PriceSourceId required.", nameof(priceSourceId));

        return new PriceCollectionJob
        {
            PriceSourceId = priceSourceId,
            Mode = PriceCollectionMode.Backfill,
            State = PriceCollectionJobState.Created,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void MarkRunningRecurring(Guid schedulerRecurringId, DateTime startedAt)
    {
        if (State == PriceCollectionJobState.Running && SchedulerRecurringId == schedulerRecurringId)
            return; // idempotent

        if (Mode != PriceCollectionMode.Recurring)
            throw new InvalidOperationException($"Cannot mark Recurring for mode {Mode}.");

        State = PriceCollectionJobState.Running;
        SchedulerRecurringId = schedulerRecurringId;
        StartedAt = startedAt;
        StoppedAt = null;
        LastError = null;
        Touch();
    }

    public void MarkRunningBackfill(Guid schedulerJobId, DateTime startedAt)
    {
        if (Mode != PriceCollectionMode.Backfill)
            throw new InvalidOperationException($"Cannot mark Backfill for mode {Mode}.");

        State = PriceCollectionJobState.Running;
        SchedulerJobId = schedulerJobId;
        StartedAt = startedAt;
        StoppedAt = null;
        LastError = null;
        BackfillProgressPercent = 0m;
        Touch();
    }

    public void RecordRun(int inserted, int skipped, DateTime at)
    {
        LastRunAt = at;
        LastRunInserted = inserted;
        LastRunSkipped = skipped;
        TotalInserted += inserted;
        Touch();
    }

    public void UpdateBackfillProgress(decimal pct)
    {
        if (Mode != PriceCollectionMode.Backfill)
            throw new InvalidOperationException("Progress only for Backfill.");
        BackfillProgressPercent = Math.Clamp(pct, 0m, 100m);
        Touch();
    }

    public void MarkStopped(DateTime at)
    {
        State = PriceCollectionJobState.Stopped;
        StoppedAt = at;
        SchedulerRecurringId = null;
        SchedulerJobId = null;
        Touch();
    }

    public void MarkCompleted(DateTime at)
    {
        if (Mode != PriceCollectionMode.Backfill)
            throw new InvalidOperationException("Completed only for Backfill.");

        State = PriceCollectionJobState.Completed;
        StoppedAt = at;
        SchedulerJobId = null;
        BackfillProgressPercent = 100m;
        Touch();
    }

    public void MarkFaulted(string error)
    {
        State = PriceCollectionJobState.Faulted;
        LastError = error;
        Touch();
    }
}