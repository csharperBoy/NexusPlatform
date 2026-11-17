
// Trader.Application/Dtos/Tuning/StrategyTuningRunSummaryView.cs
namespace Trader.Application.Dtos.Tuning;

public sealed record StrategyTuningRunSummaryView(
    Guid Id, string Name, string StrategyKey, string Isin,
    int PrimaryTimeFrame, string PrimaryTimeFrameName,
    DateTime DateFrom, DateTime DateTo,
    int Status, string StatusName,
    int TotalEvaluations, int CompletedEvaluations,
    DateTime? StartedAt, DateTime? CompletedAt, DateTime CreatedAt);
