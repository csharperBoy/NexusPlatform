namespace Trader.Application.Dtos.Backtests;

/// <summary>
/// Config کامل برای ساخت یک Virtual Backtest (بدون SourceRunId).
/// برای Existing Run، این فیلد null است و از Run کپی می‌شود.
/// </summary>
public sealed record RunBacktestConfigModel(
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    decimal? StopLossPercentage,
    decimal? StopLossPrice,
    decimal? TakeProfitPercentage,
    decimal? TakeProfitPrice,
    int DefaultOrderMode,
    int OnMarketClose,
    int OnStop,
    List<BacktestSymbolModel> Symbols);