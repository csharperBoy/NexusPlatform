
// Trader.Application/Dtos/Tuning/StrategyTuningRunDetailView.cs
namespace Trader.Application.Dtos.Tuning;

public sealed record StrategyTuningRunDetailView(
    Guid Id, string Name, string StrategyKey, string Isin, string? TargetCurrency,
    int PrimaryTimeFrame, int? SubCandleTimeFrame,
    DateTime DateFrom, DateTime DateTo,
    decimal InitialCapital, decimal SlippagePercent,
    int SelectedScales, int PrimaryScale, int MinTradesPerWindow,
    int PopulationSize, int Generations, decimal MutationRate, decimal CrossoverRate,
    int Status, string StatusName,
    int TotalEvaluations, int CompletedEvaluations,
    Guid? BestResultIdYearly, Guid? BestResultIdSemi,
    Guid? BestResultIdQuarterly, Guid? BestResultIdMonthly,
    string? ErrorMessage,
    DateTime? StartedAt, DateTime? CompletedAt, DateTime CreatedAt,
    IReadOnlyList<StrategyTuningResultView> TopResults);
