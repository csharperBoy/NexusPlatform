namespace Trader.Application.Dtos.PriceCollection;

public sealed record PriceCollectionJobView(
    Guid Id,
    Guid PriceSourceId,
    string PriceSourceCode,
    string PriceSourceName,
    int Mode,
    string ModeName,
    int State,
    string StateName,
    string? RecurrencePatternJson,
    Guid? SchedulerRecurringId,
    Guid? SchedulerJobId,
    DateTime? StartedAt,
    DateTime? StoppedAt,
    DateTime? LastRunAt,
    int LastRunInserted,
    int LastRunSkipped,
    int TotalInserted,
    decimal? BackfillProgressPercent,
    string? LastError,
    DateTime CreatedAt);