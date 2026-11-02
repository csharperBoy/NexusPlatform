namespace Trader.Application.Dtos.Backtests;

public sealed record RunBacktestModel(
    string Name,
    Guid? SourceRunId,
    RunBacktestConfigModel? ConfigOverride,
    DateTime DateFrom,
    DateTime DateTo,
    decimal? InitialCapital,
    decimal? SlippagePercent,
    int? SubCandleTimeFrame,
    string? TimeZoneId);