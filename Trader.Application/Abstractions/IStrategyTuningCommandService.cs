// Trader.Application/Abstractions/IStrategyTuningCommandService.cs
using Trader.Application.Dtos.Tuning;

namespace Trader.Application.Abstractions;

public interface IStrategyTuningCommandService
{
    Task<Guid> CreateAsync(CreateStrategyTuningModel model, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}