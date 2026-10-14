using Trader.Application.Dtos.TradingRuns;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions;

public interface ITradingRunCommandService
{
    Task<Guid> CreateAsync(CreateTradingRunModel model);
    Task UpdateAsync(Guid id, UpdateTradingRunModel model);
    Task DeleteAsync(Guid id);
    Task StartAsync(Guid id);
    Task StopAsync(Guid id, StopRunBehavior behavior);
}