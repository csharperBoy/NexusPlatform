namespace Trader.Application.Dtos.Dashboard;

public sealed record DashboardSummaryView(
    KillSwitchSummary KillSwitch,
    RunsSummary Runs,
    OrdersTodaySummary OrdersToday,
    PositionsSummary Positions,
    IReadOnlyList<CriticalEventView> RecentCriticalEvents,
    AccountsSessionSummary AccountsSessions,
    DateTime GeneratedAt);

public sealed record KillSwitchSummary(
    bool GlobalActive,
    string? GlobalReason,
    DateTime? GlobalActivatedAt,
    IReadOnlyList<KillSwitchAccountView> ActiveAccounts);

public sealed record KillSwitchAccountView(
    Guid AccountId,
    string AccountName,
    string? Reason,
    DateTime ActivatedAt);

public sealed record RunsSummary(
    int Running,
    int Ready,
    int Stopped,
    int Faulted,
    int Created)
{
    public int Total => Running + Ready + Stopped + Faulted + Created;
}

public sealed record OrdersTodaySummary(
    DateTime TodayStartUtc,
    int Total,
    int Paper,
    int Live,
    int Pending,
    int Open,
    int PartiallyExecuted,
    int FullyExecuted,
    int Canceled,
    int Rejected,
    int Expired);

public sealed record PositionsSummary(
    int TotalCount,
    int PaperCount,
    int LiveCount,
    decimal TotalUnrealizedPnL,
    IReadOnlyList<PositionBriefView> Items);

public sealed record PositionBriefView(
    Guid Id,
    Guid RunId,
    Guid AccountId,
    string AccountName,
    string Isin,
    string SymbolName,
    long Quantity,
    decimal AverageEntryPrice,
    decimal? CurrentPrice,
    decimal? UnrealizedPnL,
    bool IsPaper);

public sealed record CriticalEventView(
    Guid Id,
    DateTime OccurredAt,
    int Severity,
    string SeverityName,
    int Type,
    string TypeName,
    string? Isin,
    Guid? RunId,
    string Message);

public sealed record AccountsSessionSummary(
    int Total,
    int Valid,
    int Expired,
    int Empty,
    int Invalid);