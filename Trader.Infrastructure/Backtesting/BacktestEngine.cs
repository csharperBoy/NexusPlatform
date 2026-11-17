using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Specifications.Backtests;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Backtesting;

/// <summary>
/// orchestrator — مسئول:
///   1. Load BacktestRun از DB
///   2. MarkRunning
///   3. Build BacktestSimulationRequest
///   4. Call IBacktestSimulator (pure، بدون persist)
///   5. Persist trades/equities + MarkCompleted
///   6. Handle cancel/fail
///
/// منطق pure simulation در BacktestSimulator منتقل شده — این کلاس
/// فقط persistence + lifecycle است.
/// </summary>
public sealed class BacktestEngine : IBacktestEngine
{
    private readonly IBacktestSimulator _simulator;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IBacktestEventLogger _eventLogger;
    private readonly ILogger<BacktestEngine> _logger;

    public BacktestEngine(
        IBacktestSimulator simulator,
        ITraderUnitOfWork<TraderDbContext> uow,
        IBacktestEventLogger eventLogger,
        ILogger<BacktestEngine> logger)
    {
        _simulator = simulator;
        _uow = uow;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task RunAsync(Guid backtestRunId, CancellationToken ct = default)
    {
        // Backtest loading can exceed 30s default timeout
        _uow.Context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        var spec = new BacktestRunByIdWithSymbolsSpec(backtestRunId);
        var run = await _uow.BacktestRunSpecs.GetBySpecAsync(spec);
        if (run is null)
        {
            _logger.LogWarning("BacktestRun {Id} not found.", backtestRunId);
            return;
        }
        if (run.Status != BacktestStatus.Pending)
        {
            _logger.LogWarning("BacktestRun {Id} status={Status}, expected Pending.",
                backtestRunId, run.Status);
            return;
        }

        run.MarkRunning();
        await _uow.BacktestRuns.UpdateAsync(run);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "BacktestRun {Id} started. strategy={Strategy} symbols={Symbols} range={From:yyyy-MM-dd}→{To:yyyy-MM-dd}",
            run.Id, run.StrategyKey, run.Symbols.Count, run.DateFrom, run.DateTo);

        _eventLogger.Log(run.Id, BacktestEventType.RunStarted, BacktestEventSeverity.Info,
            $"بکتست شروع شد. استراتژی: {run.StrategyKey}, نمادها: {run.Symbols.Count}, " +
            $"بازه: {run.DateFrom:yyyy-MM-dd} تا {run.DateTo:yyyy-MM-dd}",
            payload: new { strategy = run.StrategyKey, symbolCount = run.Symbols.Count });

        try
        {
            // ─── Build Request from Entity ───
            var request = new BacktestSimulationRequest(
                StrategyKey: run.StrategyKey,
                StrategyConfigJson: run.StrategyConfigJson,
                PrimaryTimeFrame: (TimeFrame)run.PrimaryTimeFrame,
                SubCandleTimeFrame: run.SubCandleTimeFrame.HasValue
                    ? (TimeFrame)run.SubCandleTimeFrame.Value : null,
                TargetCurrency: run.TargetCurrency,
                DateFrom: run.DateFrom,
                DateTo: run.DateTo,
                InitialCapital: run.InitialCapital,
                SlippagePercent: run.SlippagePercent,
                StrengthMultiplier: run.StrengthMultiplier,
                MinDeltaPercent: run.MinDeltaPercent,
                StopLossPercentage: run.StopLossPercentage,
                StopLossPrice: run.StopLossPrice,
                TakeProfitPercentage: run.TakeProfitPercentage,
                TakeProfitPrice: run.TakeProfitPrice,
                Symbols: run.Symbols
                    .Select(s => new BacktestSimulationSymbol(s.Isin, s.AllocationPercentage))
                    .ToList());

            var result = await _simulator.SimulateAsync(request, _eventLogger, ct);

            // ─── Cancel check ───
            await _uow.Context.Entry(run).ReloadAsync(ct);
            if (run.Status != BacktestStatus.Running)
            {
                _logger.LogInformation(
                    "BacktestRun {Id} status is {Status} — cancelled during simulation; " +
                    "discarding {TradeCount} trades and {EquityCount} daily equities",
                    run.Id, run.Status, result.Trades.Count, result.DailyEquities.Count);
                await _eventLogger.FlushAsync(ct);
                return;
            }

            // ─── Persist trades + equities ───
            // Simulator used runId=Guid.Empty. Rewrite FK before persist.
            var trades = result.Trades
                .Select(t => BacktestTrade.Create(
                    backtestRunId: run.Id, isin: t.Isin,
                    entryTime: t.EntryTime, lastEntryTime: t.LastEntryTime,
                    totalEntryQuantity: t.TotalEntryQuantity,
                    weightedAvgEntryPrice: t.WeightedAvgEntryPrice,
                    totalBuyCommission: t.TotalBuyCommission,
                    entryFillsJson: t.EntryFillsJson,
                    exitTime: t.ExitTime, exitPrice: t.ExitPrice,
                    exitQuantity: t.ExitQuantity,
                    totalSellCommission: t.TotalSellCommission,
                    exitReason: t.ExitReason))
                .ToList();

            var equities = result.DailyEquities
                .Select(e => BacktestDailyEquity.Create(
                    backtestRunId: run.Id, date: e.Date,
                    equity: e.Equity, cash: e.Cash, positionValue: e.PositionValue,
                    dailyReturnPct: e.DailyReturnPct))
                .ToList();

            foreach (var trade in trades) await _uow.BacktestTrades.AddAsync(trade);
            foreach (var eq in equities) await _uow.BacktestDailyEquities.AddAsync(eq);

            run.MarkCompleted(
                finalEquity: result.FinalCash,
                totalReturnPct: result.Metrics.TotalReturnPct,
                maxDrawdownPct: result.Metrics.MaxDrawdownPct,
                sharpeRatio: result.Metrics.SharpeRatio,
                winRate: result.Metrics.WinRate,
                totalTrades: result.Metrics.TotalTrades,
                winningTrades: result.Metrics.WinningTrades,
                losingTrades: result.Metrics.LosingTrades,
                profitFactor: result.Metrics.ProfitFactor,
                avgWin: result.Metrics.AvgWin,
                avgLoss: result.Metrics.AvgLoss,
                totalCommission: result.TotalCommission,
                totalSlippage: result.TotalSlippage);

            await _uow.BacktestRuns.UpdateAsync(run);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "BacktestRun {Id} completed. return={ReturnPct:0.##}% trades={Trades} duration={Dur}ms",
                run.Id, result.Metrics.TotalReturnPct, result.Metrics.TotalTrades, run.DurationMs);

            _eventLogger.Log(run.Id, BacktestEventType.MetricsComputed, BacktestEventSeverity.Info,
                $"متریک‌ها محاسبه شدند. بازده: {result.Metrics.TotalReturnPct:0.##}%, " +
                $"تریدها: {result.Metrics.TotalTrades}",
                payload: result.Metrics);

            _eventLogger.Log(run.Id, BacktestEventType.RunCompleted, BacktestEventSeverity.Info,
                $"بکتست تکمیل شد. سرمایه‌ی نهایی: {result.FinalCash:N0}, مدت: {run.DurationMs}ms",
                payload: new
                {
                    finalEquity = result.FinalCash,
                    totalReturnPct = result.Metrics.TotalReturnPct,
                    totalTrades = result.Metrics.TotalTrades,
                    durationMs = run.DurationMs,
                });

            await _eventLogger.FlushAsync(ct);
        }
        catch (OperationCanceledException)
        {
            await _uow.Context.Entry(run).ReloadAsync(CancellationToken.None);
            if (run.Status == BacktestStatus.Running)
            {
                run.MarkCancelled();
                await _uow.BacktestRuns.UpdateAsync(run);
                await _uow.SaveChangesAsync(CancellationToken.None);
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BacktestRun {Id} failed.", run.Id);

            await _uow.Context.Entry(run).ReloadAsync(CancellationToken.None);
            if (run.Status == BacktestStatus.Running)
            {
                _eventLogger.Log(run.Id, BacktestEventType.RunFailed, BacktestEventSeverity.Error,
                    $"بکتست ناموفق بود: {ex.Message}",
                    payload: new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message });

                run.MarkFailed(ex.Message);
                await _uow.BacktestRuns.UpdateAsync(run);
                await _uow.SaveChangesAsync(CancellationToken.None);
            }
            await _eventLogger.FlushAsync(CancellationToken.None);
            throw;
        }
    }
}