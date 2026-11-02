using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

/// <summary>
/// Job برای جمع‌آوری Historical Candles یک یا چند نماد در یک یا چند TimeFrame.
/// On-Demand — از Command شروع می‌شود.
/// </summary>
public class HistoricalDataCollectionJob : BaseEntity, IAuditableEntity
{
    public string Name { get; private set; } = default!;
    public CollectionJobStatus Status { get; private set; }

    /// <summary>JSON array از ISINها.</summary>
    public string IsinsJson { get; private set; } = "[]";

    /// <summary>JSON array از TimeFrame enum intها.</summary>
    public string TimeFramesJson { get; private set; } = "[]";

    public DateTime DateFrom { get; private set; }
    public DateTime DateTo { get; private set; }

    // Progress
    public int TotalCombinations { get; private set; }       // Isins × TimeFrames
    public int CompletedCombinations { get; private set; }
    public int TotalCandlesCollected { get; private set; }
    public decimal ProgressPercent { get; private set; }
    public string? CurrentIsin { get; private set; }
    public int? CurrentTimeFrame { get; private set; }

    // Timing
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }

    // Errors / Log
    public string? ErrorMessage { get; private set; }

    /// <summary>JSON array از log entries: [{time, level, message}, ...]</summary>
    public string LogMessagesJson { get; private set; } = "[]";

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private HistoricalDataCollectionJob() { } // EF

    public static HistoricalDataCollectionJob Create(
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

        return new HistoricalDataCollectionJob
        {
            Name = name.Trim(),
            Status = CollectionJobStatus.Pending,
            IsinsJson = isinsJson,
            TimeFramesJson = timeFramesJson,
            DateFrom = dateFrom.Kind == DateTimeKind.Utc
                ? dateFrom
                : DateTime.SpecifyKind(dateFrom, DateTimeKind.Utc),
            DateTo = dateTo.Kind == DateTimeKind.Utc
                ? dateTo
                : DateTime.SpecifyKind(dateTo, DateTimeKind.Utc),
            TotalCombinations = totalCombinations,
            CompletedCombinations = 0,
            TotalCandlesCollected = 0,
            ProgressPercent = 0m,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkRunning()
    {
        if (Status != CollectionJobStatus.Pending)
            throw new InvalidOperationException($"Cannot start from status {Status}.");
        Status = CollectionJobStatus.Running;
        StartedAt = DateTime.UtcNow;
        Touch();
    }

    public void UpdateProgress(
        string currentIsin,
        int currentTimeFrame,
        int completedCombinations,
        int candlesCollected)
    {
        CurrentIsin = currentIsin;
        CurrentTimeFrame = currentTimeFrame;
        CompletedCombinations = completedCombinations;
        TotalCandlesCollected = candlesCollected;
        ProgressPercent = TotalCombinations > 0
            ? Math.Round((decimal)completedCombinations / TotalCombinations * 100m, 2)
            : 0m;
        Touch();
    }

    public void AppendLog(string logJson)
    {
        LogMessagesJson = logJson;
        Touch();
    }

    public void MarkCompleted()
    {
        Status = CollectionJobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        DurationMs = StartedAt.HasValue
            ? (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds
            : null;
        ProgressPercent = 100m;
        Touch();
    }

    public void MarkFailed(string errorMessage)
    {
        Status = CollectionJobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        DurationMs = StartedAt.HasValue
            ? (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds
            : null;
        Touch();
    }

    public void MarkCancelled()
    {
        Status = CollectionJobStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        DurationMs = StartedAt.HasValue
            ? (long)(CompletedAt.Value - StartedAt.Value).TotalMilliseconds
            : null;
        Touch();
    }
}