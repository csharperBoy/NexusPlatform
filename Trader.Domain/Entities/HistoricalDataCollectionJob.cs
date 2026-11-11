using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

/// <summary>
/// Job جمع‌آوری Candles تاریخی.
///
/// Lifecycle:
///   Backfill: Created → (Start) → Running → (تمام) → Completed
///             یا → Faulted | Stopped
///   Recurring: Created → (Start) → Running ↔ Stopped
///              (هر tick: RecordRecurringRun، در Running می‌ماند)
///
/// Boundary semantics:
///   Backfill: DateFrom..DateTo (ثابت، از کاربر)
///   Recurring: max(OpenTime) per (Isin, TF) در DB → now
///              اگر هیچ رکوردی نبود: DateFrom (از کاربر) → now
/// </summary>
public class HistoricalDataCollectionJob : BaseEntity, IAuditableEntity
{
    /* ═══════════ Identity ═══════════ */

    public string Name { get; private set; } = default!;
    public HistoricalCollectionMode Mode { get; private set; }
    public HistoricalJobState State { get; private set; }

    /* ═══════════ Config ═══════════ */

    /// <summary>JSON array از ISINها.</summary>
    public string IsinsJson { get; private set; } = "[]";

    /// <summary>JSON array از TimeFrame enum intها (1440, 60, ...).</summary>
    public string TimeFramesJson { get; private set; } = "[]";

    /// <summary>پایین‌ترین تاریخ. Backfill: حتماً. Recurring: fallback اگر هیچ کندلی در DB نبود.</summary>
    public DateTime DateFrom { get; private set; }

    /// <summary>بالاترین تاریخ. Backfill: از کاربر. Recurring: null (همیشه now).</summary>
    public DateTime? DateTo { get; private set; }

    /// <summary>JSON RecurrencePattern — فقط برای Mode=Recurring. Backfill null.</summary>
    public string? RecurrencePatternJson { get; private set; }

    /* ═══════════ Scheduler ═══════════ */

    /// <summary>Guid بازگشتی از ISchedulerService.ScheduleRecurringAsync. null = non-recurring.</summary>
    public Guid? SchedulerRecurringId { get; private set; }

    /// <summary>Guid بازگشتی از ISchedulerService.ScheduleAsync. null = non-backfill.</summary>
    public Guid? SchedulerJobId { get; private set; }

    /* ═══════════ Progress ═══════════ */

    public int TotalCombinations { get; private set; }              // isins × tfs
    public int CompletedCombinations { get; private set; }          // در اجرای فعلی (Backfill)
    public int TotalCandlesCollected { get; private set; }          // lifetime
    public decimal? BackfillProgressPercent { get; private set; }   // Backfill only
    public string? CurrentIsin { get; private set; }
    public int? CurrentTimeFrame { get; private set; }

    /* ═══════════ Run Stats ═══════════ */

    public DateTime? StartedAt { get; private set; }
    public DateTime? StoppedAt { get; private set; }
    public DateTime? LastRunAt { get; private set; }
    public int LastRunInserted { get; private set; }
    public int LastRunSkipped { get; private set; }

    /* ═══════════ Errors / Completion ═══════════ */

    public string? LastError { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }

    /// <summary>Audit log entries — JSON array. نگه‌داری برای backward compat.</summary>
    public string LogMessagesJson { get; private set; } = "[]";

    /* ═══════════ IAuditableEntity ═══════════ */

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private HistoricalDataCollectionJob() { } // EF

    /* ═══════════ Factories ═══════════ */

    public static HistoricalDataCollectionJob CreateBackfill(
        string name,
        string isinsJson,
        string timeFramesJson,
        DateTime dateFrom,
        DateTime dateTo,
        int totalCombinations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (dateTo <= dateFrom)
            throw new InvalidOperationException("DateTo must be after DateFrom.");
        if (totalCombinations <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalCombinations));

        return new HistoricalDataCollectionJob
        {
            Name = name.Trim(),
            Mode = HistoricalCollectionMode.Backfill,
            State = HistoricalJobState.Created,
            IsinsJson = isinsJson,
            TimeFramesJson = timeFramesJson,
            DateFrom = ToUtc(dateFrom),
            DateTo = ToUtc(dateTo),
            RecurrencePatternJson = null,
            TotalCombinations = totalCombinations,
            CompletedCombinations = 0,
            TotalCandlesCollected = 0,
            LogMessagesJson = "[]",
            CreatedAt = DateTime.UtcNow,
        };
    }

    public static HistoricalDataCollectionJob CreateRecurring(
        string name,
        string isinsJson,
        string timeFramesJson,
        DateTime dateFrom,
        string recurrencePatternJson,
        int totalCombinations)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(recurrencePatternJson))
            throw new ArgumentException(
                "RecurrencePatternJson is required for Recurring.", nameof(recurrencePatternJson));
        if (totalCombinations <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalCombinations));

        return new HistoricalDataCollectionJob
        {
            Name = name.Trim(),
            Mode = HistoricalCollectionMode.Recurring,
            State = HistoricalJobState.Created,
            IsinsJson = isinsJson,
            TimeFramesJson = timeFramesJson,
            DateFrom = ToUtc(dateFrom),
            DateTo = null,
            RecurrencePatternJson = recurrencePatternJson.Trim(),
            TotalCombinations = totalCombinations,
            CompletedCombinations = 0,
            TotalCandlesCollected = 0,
            LogMessagesJson = "[]",
            CreatedAt = DateTime.UtcNow,
        };
    }

    /* ═══════════ State Machine ═══════════ */

    public void MarkRunningBackfill(Guid schedulerJobId, DateTime startedAt)
    {
        if (State == HistoricalJobState.Running && SchedulerJobId == schedulerJobId)
            return; // idempotent

        if (Mode != HistoricalCollectionMode.Backfill)
            throw new InvalidOperationException(
                $"Cannot mark Backfill for mode {Mode}.");

        State = HistoricalJobState.Running;
        SchedulerJobId = schedulerJobId;
        StartedAt = ToUtc(startedAt);
        StoppedAt = null;
        LastError = null;
        BackfillProgressPercent = 0m;
        CompletedCombinations = 0;
        Touch();
    }

    public void MarkRunningRecurring(Guid schedulerRecurringId, DateTime startedAt)
    {
        if (State == HistoricalJobState.Running
            && SchedulerRecurringId == schedulerRecurringId)
            return;

        if (Mode != HistoricalCollectionMode.Recurring)
            throw new InvalidOperationException(
                $"Cannot mark Recurring for mode {Mode}.");

        State = HistoricalJobState.Running;
        SchedulerRecurringId = schedulerRecurringId;
        StartedAt = ToUtc(startedAt);
        StoppedAt = null;
        LastError = null;
        Touch();
    }

    /// <summary>پیشرفت Backfill — per-combo، حین اجرا.</summary>
    public void RecordBackfillProgress(
        string currentIsin, int currentTf,
        int completedCombinations, int totalCandles)
    {
        if (Mode != HistoricalCollectionMode.Backfill)
            throw new InvalidOperationException("Progress only for Backfill.");

        CurrentIsin = currentIsin;
        CurrentTimeFrame = currentTf;
        CompletedCombinations = completedCombinations;
        TotalCandlesCollected = totalCandles;
        BackfillProgressPercent = TotalCombinations > 0
            ? Math.Round((decimal)completedCombinations / TotalCombinations * 100m, 2)
            : 0m;
        Touch();
    }

    /// <summary>گزارش یک tick Recurring (یا یک اجرای Backfill در انتها).</summary>
    public void RecordRecurringRun(int inserted, int skipped, DateTime at)
    {
        if (Mode != HistoricalCollectionMode.Recurring)
            throw new InvalidOperationException("RecordRecurringRun only for Recurring.");

        LastRunAt = ToUtc(at);
        LastRunInserted = inserted;
        LastRunSkipped = skipped;
        TotalCandlesCollected += inserted;
        CurrentIsin = null;
        CurrentTimeFrame = null;
        Touch();
    }

    public void MarkStopped(DateTime at)
    {
        if (State != HistoricalJobState.Running)
            return; // no-op if already terminal

        State = HistoricalJobState.Stopped;
        StoppedAt = ToUtc(at);
        SchedulerRecurringId = null;
        SchedulerJobId = null;
        Touch();
    }

    public void MarkCompleted(DateTime at)
    {
        if (Mode != HistoricalCollectionMode.Backfill)
            throw new InvalidOperationException("Completed only for Backfill.");

        State = HistoricalJobState.Completed;
        CompletedAt = ToUtc(at);
        SchedulerJobId = null;
        BackfillProgressPercent = 100m;
        DurationMs = StartedAt.HasValue
            ? (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds
            : null;
        Touch();
    }

    public void MarkFaulted(string error)
    {
        State = HistoricalJobState.Faulted;
        LastError = error;
        Touch();
    }

    public void AppendLog(string logJson)
    {
        LogMessagesJson = logJson;
        Touch();
    }

    /* ═══════════ Helpers ═══════════ */

    private static DateTime ToUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}