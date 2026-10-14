using Trader.Application.Dtos.Strategies;

namespace Trader.Application.Abstractions;

public interface IStrategyQueryService
{
    IReadOnlyList<StrategyInfoView> GetRegisteredStrategies();
}