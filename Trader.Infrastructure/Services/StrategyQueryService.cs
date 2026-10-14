using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Dtos.Strategies;

namespace Trader.Application.Services;

public class StrategyQueryService : IStrategyQueryService
{
    private readonly IStrategyFactory _factory;

    public StrategyQueryService(IStrategyFactory factory)
    {
        _factory = factory;
    }

    public IReadOnlyList<StrategyInfoView> GetRegisteredStrategies()
    {
        return _factory.GetRegisteredStrategies()
            .Select(d => new StrategyInfoView(
                d.Key,
                d.DisplayName,
                d.ConfigType.Name,
                ExtractConfigFields(d.ConfigType)))
            .ToList();
    }

    private static IReadOnlyList<StrategyConfigFieldView> ExtractConfigFields(Type configType)
    {
        return configType
            .GetProperties()
            .Where(p => p.Name != nameof(StrategyConfig.StrategyKey))
            .Select(p => new StrategyConfigFieldView(
                p.Name,
                p.PropertyType.Name,
                IsRequired: false,
                DefaultValue: null))
            .ToList();
    }
}