using Trader.Application.Dtos.TradingSchedules;

namespace Trader.Application.Abstractions;

public interface ITradingScheduleQueryService
{
    Task<IReadOnlyList<TradingScheduleInfoView>> GetListAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TradingScheduleSelectionView>> GetSelectionListAsync(CancellationToken ct = default);
}