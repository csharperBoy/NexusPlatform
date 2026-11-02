using Trader.Application.Dtos.Backtests;

namespace Trader.Application.Abstractions.Backtesting;

public interface IBacktestRunCommandService
{
    Task<Guid> CreateAsync(RunBacktestModel model, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid id, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}