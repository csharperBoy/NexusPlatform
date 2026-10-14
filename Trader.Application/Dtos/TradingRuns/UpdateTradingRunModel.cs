namespace Trader.Application.Dtos.TradingRuns;

public sealed record UpdateTradingRunModel(
    string Name,
    string StrategyKey,
    string StrategyConfigJson,
    int PrimaryTimeFrame,
    TradingRunSettingsModel Settings,
    IReadOnlyList<TradingRunSymbolModel> Symbols,
    IReadOnlyList<Guid> AccountIds);