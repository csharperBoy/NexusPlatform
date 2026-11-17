// Trader.Application/Abstractions/IStrategyTuningQueryService.cs
using Trader.Application.Dtos.Tuning;

namespace Trader.Application.Abstractions;

public interface IStrategyTuningQueryService
{
    Task<IReadOnlyList<StrategyTuningRunSummaryView>> GetListAsync(int take = 100, CancellationToken ct = default);
    Task<StrategyTuningRunDetailView?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<StrategyTuningResultView>> GetResultsAsync(
        Guid tuningRunId, bool topOnly = false, int take = 100, CancellationToken ct = default);
}