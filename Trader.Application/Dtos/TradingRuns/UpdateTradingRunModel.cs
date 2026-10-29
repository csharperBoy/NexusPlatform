namespace Trader.Application.Dtos.TradingRuns;

public sealed record UpdateTradingRunModel(
    string Name,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    string? TargetCurrency,
    decimal StrengthMultiplier,
    TradingRunSettingsModel Settings,
    IReadOnlyList<TradingRunSymbolModel> Symbols,
    IReadOnlyList<Guid> AccountIds);