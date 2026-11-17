using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// ورودی pure simulation — همه‌چیز لازم برای اجرای Backtest در حافظه،
/// بدون وابستگی به DB (BacktestRun entity) یا scope.
///
/// استفاده:
///   • BacktestEngine — از BacktestRun می‌سازد و برای persist استفاده می‌کند
///   • GeneticTunerExecutor — برای هر ارزیابی chromosome، یکی می‌سازد
///
/// نکته: چند نماد پشتیبانی می‌شوند (Symbols)، ولی tuning از تک‌نماد
/// با AllocationPercentage=100 استفاده می‌کند.
/// </summary>
public sealed record BacktestSimulationRequest(
    string StrategyKey,
    string StrategyConfigJson,
    TimeFrame PrimaryTimeFrame,
    TimeFrame? SubCandleTimeFrame,
    string? TargetCurrency,
    DateTime DateFrom,
    DateTime DateTo,
    decimal InitialCapital,
    decimal SlippagePercent,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    decimal? StopLossPercentage,
    decimal? StopLossPrice,
    decimal? TakeProfitPercentage,
    decimal? TakeProfitPrice,
    IReadOnlyList<BacktestSimulationSymbol> Symbols);

public sealed record BacktestSimulationSymbol(
    string Isin,
    decimal AllocationPercentage);