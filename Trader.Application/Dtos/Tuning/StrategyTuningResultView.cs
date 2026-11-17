// Trader.Application/Dtos/Tuning/StrategyTuningResultView.cs
namespace Trader.Application.Dtos.Tuning;

public sealed record StrategyTuningResultView(
    Guid Id, int Generation, int Status,
    string ConfigJson,
    decimal? AvgReturnCompoundYearly, decimal? AvgReturnCompoundSemi,
    decimal? AvgReturnCompoundQuarterly, decimal? AvgReturnCompoundMonthly,
    int? ValidWindowsYearly, int? ValidWindowsSemi,
    int? ValidWindowsQuarterly, int? ValidWindowsMonthly,
    int? TotalTradesYearly, int? TotalTradesSemi,
    int? TotalTradesQuarterly, int? TotalTradesMonthly,
    bool IsTop10InYearly, bool IsTop10InSemi,
    bool IsTop10InQuarterly, bool IsTop10InMonthly,
    long RuntimeMs);