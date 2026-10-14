namespace Trader.Application.Dtos.Strategies;

public sealed record StrategyInfoView(
    string Key,
    string DisplayName,
    string ConfigTypeName,
    IReadOnlyList<StrategyConfigFieldView> ConfigFields);

public sealed record StrategyConfigFieldView(
    string Name,
    string TypeName,
    bool IsRequired,
    string? DefaultValue);