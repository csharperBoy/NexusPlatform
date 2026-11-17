using Trader.Domain.Entities;

namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// خروجی pure simulation. هیچ‌چیز persist نمی‌شود — caller تصمیم می‌گیرد.
///
/// نکته: BacktestTrade.BacktestRunId و BacktestDailyEquity.BacktestRunId
/// برای tuning = Guid.Empty (persist نمی‌شوند). BacktestEngine قبل از
/// persist، Guid واقعی رو set می‌کند.
/// </summary>
public sealed record BacktestSimulationResult(
    decimal InitialCapital,
    decimal FinalCash,
    IReadOnlyList<BacktestTrade> Trades,
    IReadOnlyList<BacktestDailyEquity> DailyEquities,
    decimal TotalCommission,
    decimal TotalSlippage,
    BacktestMetrics Metrics);