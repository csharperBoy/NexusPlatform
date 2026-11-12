using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Backtests;
using Trader.Application.Specifications.Candles;
using Trader.Application.Specifications.Instruments;
using Trader.Domain.Calculations;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Backtesting;

public sealed class BacktestEngine : IBacktestEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISubCandleResolver _sltpResolver;
    private readonly IOrderFillSimulator _fillSimulator;
    private readonly ILogger<BacktestEngine> _logger;
    private readonly IOptionsMonitor<TradingRiskOptions> _riskOptions;

    public BacktestEngine(
        IServiceScopeFactory scopeFactory,
        ISubCandleResolver sltpResolver,
        IOrderFillSimulator fillSimulator,
    IOptionsMonitor<TradingRiskOptions> riskOptions,
        ILogger<BacktestEngine> logger)
    {
        _scopeFactory = scopeFactory;
        _sltpResolver = sltpResolver;
        _fillSimulator = fillSimulator;
        _riskOptions = riskOptions;
        _logger = logger;
    }

    public async Task RunAsync(Guid backtestRunId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var eventLogger = scope.ServiceProvider.GetRequiredService<IBacktestEventLogger>();

        // Backtest operations (loading historical candles) can exceed default 30s
        uow.Context.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

        var spec = new BacktestRunByIdWithSymbolsSpec(backtestRunId);
        var run = await uow.BacktestRunSpecs.GetBySpecAsync(spec);
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
        await uow.BacktestRuns.UpdateAsync(run);
        await uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "BacktestRun {Id} started. strategy={Strategy} symbols={Symbols} range={From:yyyy-MM-dd}→{To:yyyy-MM-dd}",
            run.Id, run.StrategyKey, run.Symbols.Count, run.DateFrom, run.DateTo);
        eventLogger.Log(run.Id, BacktestEventType.RunStarted, BacktestEventSeverity.Info,
            $"بکتست شروع شد. استراتژی: {run.StrategyKey}, نمادها: {run.Symbols.Count}, " +
            $"بازه: {run.DateFrom:yyyy-MM-dd} تا {run.DateTo:yyyy-MM-dd}",
            payload: new
            {
                strategy = run.StrategyKey,
                symbols = run.Symbols.Select(s => s.Isin).ToArray(),
                dateFrom = run.DateFrom,
                dateTo = run.DateTo,
                initialCapital = run.InitialCapital
            });
        try
        {
            var state = await SimulateAsync(run, uow, eventLogger, ct);

            // Check if user cancelled during simulation (via CancelBacktestCommand
            // in another scope). Reload to see the current DB status.
            await uow.Context.Entry(run).ReloadAsync(ct);
            if (run.Status != BacktestStatus.Running)
            {
                _logger.LogInformation(
                    "BacktestRun {Id} status is {Status} — cancelled during simulation; " +
                    "discarding {TradeCount} trades and {EquityCount} daily equities",
                    run.Id, run.Status, state.Trades.Count, state.DailyEquities.Count);

                await eventLogger.FlushAsync(ct);
                return;
            }

            var metrics = BacktestMetricsCalculator.Calculate(
                state.InitialCapital, state.Cash, state.Trades, state.DailyEquities);

            foreach (var trade in state.Trades)
                await uow.BacktestTrades.AddAsync(trade);

            foreach (var eq in state.DailyEquities)
                await uow.BacktestDailyEquities.AddAsync(eq);

            run.MarkCompleted(
                finalEquity: state.Cash,
                totalReturnPct: metrics.TotalReturnPct,
                maxDrawdownPct: metrics.MaxDrawdownPct,
                sharpeRatio: metrics.SharpeRatio,
                winRate: metrics.WinRate,
                totalTrades: metrics.TotalTrades,
                winningTrades: metrics.WinningTrades,
                losingTrades: metrics.LosingTrades,
                profitFactor: metrics.ProfitFactor,
                avgWin: metrics.AvgWin,
                avgLoss: metrics.AvgLoss,
                totalCommission: state.TotalCommission,
                totalSlippage: state.TotalSlippage);

            await uow.BacktestRuns.UpdateAsync(run);
            await uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "BacktestRun {Id} completed. return={ReturnPct:0.##}% trades={Trades} duration={Dur}ms",
                run.Id, metrics.TotalReturnPct, metrics.TotalTrades, run.DurationMs);

            eventLogger.Log(run.Id, BacktestEventType.MetricsComputed, BacktestEventSeverity.Info,
                    $"متریک‌ها محاسبه شدند. بازده: {metrics.TotalReturnPct:0.##}%, " +
                    $"تریدها: {metrics.TotalTrades} (برد: {metrics.WinningTrades}, باخت: {metrics.LosingTrades})",
                    payload: metrics);

            eventLogger.Log(run.Id, BacktestEventType.RunCompleted, BacktestEventSeverity.Info,
                $"بکتست تکمیل شد. سرمایه‌ی نهایی: {state.Cash:N0}, مدت: {run.DurationMs}ms",
                payload: new
                {
                    finalEquity = state.Cash,
                    totalReturnPct = metrics.TotalReturnPct,
                    totalTrades = metrics.TotalTrades,
                    durationMs = run.DurationMs
                });

            await eventLogger.FlushAsync(ct);
        }
        catch (OperationCanceledException)
        {
            await uow.Context.Entry(run).ReloadAsync(CancellationToken.None);
            if (run.Status == BacktestStatus.Running)
            {
                run.MarkCancelled();
                await uow.BacktestRuns.UpdateAsync(run);
                await uow.SaveChangesAsync(CancellationToken.None);
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BacktestRun {Id} failed.", run.Id);

            await uow.Context.Entry(run).ReloadAsync(CancellationToken.None);
            if (run.Status == BacktestStatus.Running)
            {
                eventLogger.Log(run.Id, BacktestEventType.RunFailed, BacktestEventSeverity.Error,
                    $"بکتست ناموفق بود: {ex.Message}",
                    payload: new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message });

                run.MarkFailed(ex.Message);
                await uow.BacktestRuns.UpdateAsync(run);
                await uow.SaveChangesAsync(CancellationToken.None);
            }
            else
            {
                _logger.LogWarning(
                    "BacktestRun {Id} exception but status is {Status}; not marking failed",
                    run.Id, run.Status);
            }

            await eventLogger.FlushAsync(CancellationToken.None);
            throw;
        }
    }

    /* ═══════════════════ Simulation ═══════════════════ */

    private async Task<BacktestState> SimulateAsync(
        BacktestRun run,
        ITraderUnitOfWork<TraderDbContext> uow,
        IBacktestEventLogger eventLogger,
        CancellationToken ct)
    {
        using var simulationScope = _scopeFactory.CreateScope();
        var sp = simulationScope.ServiceProvider;

        var strategyFactory = sp.GetRequiredService<IStrategyFactory>();
        var sizeCalc = sp.GetRequiredService<Trader.Application.Abstractions.Execution.IOrderSizeCalculator>();
        var transformer = sp.GetRequiredService<ICandleTransformer>();

        // ─── Config ───
        var strategyConfig = strategyFactory.DeserializeConfig(
                run.StrategyKey, run.StrategyConfigJson);

        var strategy = strategyFactory.Create(run.StrategyKey, strategyConfig);

        var timeFrame = (TimeFrame)run.PrimaryTimeFrame;
        var subTimeFrame = run.SubCandleTimeFrame.HasValue
            ? (TimeFrame)run.SubCandleTimeFrame.Value
            : (TimeFrame?)null;

        eventLogger.Log(run.Id, BacktestEventType.ConfigLoaded, BacktestEventSeverity.Info,
   $"Config لود شد. استراتژی: {run.StrategyKey}, " +
   $"تایم‌فریم: {timeFrame}, sub-candle: {subTimeFrame?.ToString() ?? "غیرفعال"}, " +
   $"Strength: {run.StrengthMultiplier}, MinDelta: {run.MinDeltaPercent:P1}",
   payload: new
   {
       strategyKey = run.StrategyKey,
       strategyConfigJson = run.StrategyConfigJson,
       timeFrame = timeFrame.ToString(),
       subTimeFrame = subTimeFrame?.ToString(),
       strengthMultiplier = run.StrengthMultiplier,
       minDeltaPercent = run.MinDeltaPercent
   });

        // اگر subTimeFrame >= primary، استفاده نکن
        if (subTimeFrame.HasValue && (int)subTimeFrame.Value >= (int)timeFrame)
            subTimeFrame = null;

        // ─── Instruments ───
        var symbolAllocations = run.Symbols
            .Select(s => new { s.Isin, s.AllocationPercentage })
            .ToList();

        var instruments = new Dictionary<string, Instrument>();
        foreach (var s in symbolAllocations)
        {
            var inst = await uow.InstrumentSpecs.GetBySpecAsync(
                new InstrumentByIsinSpec(s.Isin));
            if (inst is null)
                throw new InvalidOperationException(
                    $"Instrument '{s.Isin}' not found for backtest {run.Id}.");
            instruments[s.Isin] = inst;
            eventLogger.Log(run.Id, BacktestEventType.InstrumentsLoaded, BacktestEventSeverity.Debug,
    $"Instrument {s.Isin} لود شد.",
    isin: s.Isin);
        }

        // ─── Candles ───
        var primaryCandles = new Dictionary<string, List<Candle>>();
        var subCandles = subTimeFrame.HasValue
            ? new Dictionary<string, List<Candle>>()
            : null;

        foreach (var s in symbolAllocations)
        {
            var primary = await LoadCandlesAsync(
                uow, s.Isin, timeFrame, run.DateFrom, run.DateTo, ct);

            if (primary.Count < strategy.RequiredCandleCount + 1)
                throw new InvalidOperationException(
                    $"Not enough candles for {s.Isin}: have {primary.Count}, " +
                    $"need {strategy.RequiredCandleCount + 1}.");

            if (!string.IsNullOrWhiteSpace(run.TargetCurrency)
                && run.TargetCurrency != "IRR")
            {
                var tr = await transformer.TransformAsync(
                    s.Isin, primary, run.TargetCurrency, run.DateTo, ct);
                primary = tr.Candles.ToList();
            }

            primaryCandles[s.Isin] = primary;
            eventLogger.Log(run.Id, BacktestEventType.CandlesLoaded, BacktestEventSeverity.Info,
    $"کندل‌های {s.Isin} لود شدند: {primary.Count} کندل " +
    $"({primary[0].OpenTime:yyyy-MM-dd} تا {primary[^1].OpenTime:yyyy-MM-dd})",
    payload: new
    {
        isin = s.Isin,
        timeFrame = timeFrame.ToString(),
        count = primary.Count,
        firstCandle = primary[0].OpenTime,
        lastCandle = primary[^1].OpenTime
    },
    isin: s.Isin);
            if (subCandles is not null)
            {
                var sub = await LoadCandlesAsync(
                    uow, s.Isin, subTimeFrame!.Value, run.DateFrom, run.DateTo, ct);

                if (!string.IsNullOrWhiteSpace(run.TargetCurrency)
                    && run.TargetCurrency != "IRR")
                {
                    var tr = await transformer.TransformAsync(
                        s.Isin, sub, run.TargetCurrency, run.DateTo, ct);
                    sub = tr.Candles.ToList();
                }

                subCandles[s.Isin] = sub;
            }
        }

        // ─── Extra TimeFrames (Strategy requirements + SL/TP sub) ───
        var strategyReqs = strategy.ExtraCandleRequirements
            .Where(r => r.TimeFrame != timeFrame
                     && (int)r.TimeFrame < (int)timeFrame)
            .ToList();

        // Union of all needed extra TFs
        var neededExtraTfs = new HashSet<TimeFrame>();
        if (subTimeFrame.HasValue)
            neededExtraTfs.Add(subTimeFrame.Value);
        foreach (var req in strategyReqs)
            neededExtraTfs.Add(req.TimeFrame);

        // Load all extra TFs once per symbol
        var extraCandleLists = new Dictionary<(string Isin, TimeFrame Tf), List<Candle>>();
        foreach (var s in symbolAllocations)
        {
            foreach (var tf in neededExtraTfs)
            {
                var extra = await LoadCandlesAsync(
                    uow, s.Isin, tf, run.DateFrom, run.DateTo, ct);

                if (!string.IsNullOrWhiteSpace(run.TargetCurrency)
                    && run.TargetCurrency != "IRR"
                    && extra.Count > 0)
                {
                    var tr = await transformer.TransformAsync(
                        s.Isin, extra, run.TargetCurrency, run.DateTo, ct);
                    extra = tr.Candles.ToList();
                }

                extraCandleLists[(s.Isin, tf)] = extra;

                if (strategyReqs.Any(r => r.TimeFrame == tf))
                {
                    eventLogger.Log(run.Id, BacktestEventType.CandlesLoaded,
                        BacktestEventSeverity.Info,
                        $"کندل‌های Sub ({tf}) برای {s.Isin}: {extra.Count} کندل",
                        payload: new
                        {
                            isin = s.Isin,
                            timeFrame = tf.ToString(),
                            isSubCandle = true,
                            count = extra.Count
                        },
                        isin: s.Isin);
                }
            }
        }
        // ─── State ───
        var state = new BacktestState
        {
            InitialCapital = run.InitialCapital,
            Cash = run.InitialCapital
        };

        // ─── Build Events ───
        var events = new List<BacktestCandleEvent>();
        var tfDuration = timeFrame.ToDuration();

        foreach (var s in symbolAllocations)
        {
            var primList = primaryCandles[s.Isin];

            for (int i = 0; i < primList.Count; i++)
            {
                var c = primList[i];
                if (c.OpenTime < run.DateFrom || c.OpenTime > run.DateTo)
                    continue;

                IReadOnlyList<Candle>? subs = null;
                if (subCandles is not null && subCandles.ContainsKey(s.Isin))
                {
                    var end = c.OpenTime + tfDuration;
                    subs = subCandles[s.Isin]
                        .Where(sc => sc.OpenTime >= c.OpenTime && sc.OpenTime < end)
                        .ToList();
                }

                events.Add(new BacktestCandleEvent(
                    Time: c.OpenTime,
                    Isin: s.Isin,
                    Candle: c,
                    SubCandles: subs));
            }
        }

        events.Sort((a, b) =>
        {
            var cmp = a.Time.CompareTo(b.Time);
            return cmp != 0 ? cmp : string.Compare(a.Isin, b.Isin, StringComparison.Ordinal);
        });
        eventLogger.Log(run.Id, BacktestEventType.EventsBuilt, BacktestEventSeverity.Info,
    $"{events.Count:N0} رویداد شبیه‌سازی ساخته شد " +
    $"({(events.Count > 0 ? events[0].Time : DateTime.MinValue):yyyy-MM-dd} تا " +
    $"{(events.Count > 0 ? events[^1].Time : DateTime.MinValue):yyyy-MM-dd})",
    payload: new { eventCount = events.Count },
    candleTime: events.Count > 0 ? events[0].Time : (DateTime?)null);

        eventLogger.Log(run.Id, BacktestEventType.SimulationStarted, BacktestEventSeverity.Info,
            "شبیه‌سازی شروع شد.");
        // ─── Running candle list per symbol ───
        var runningCandles = symbolAllocations
            .ToDictionary(s => s.Isin, s => new List<Candle>());

        // ─── Daily equity tracking ───
        DateTime? currentDay = null;
        decimal lastEquity = state.Cash;
        // Cap history per symbol to strategy's requirement + small buffer.
        // RSI(14) is memoryless after warmup — old candles don't change result.
        var maxHistoryToKeep = strategy.RequiredCandleCount + 20;

        var totalEvents = events.Count;
        var lastProgressLog = 0;
        // ─── Simulation Loop ───
        for (int eventIdx = 0; eventIdx < events.Count; eventIdx++)
        {
            ct.ThrowIfCancellationRequested();
            var ev = events[eventIdx];

            var isin = ev.Isin;
            var candle = ev.Candle;
            var instrument = instruments[isin];
            var allocPct = symbolAllocations.First(s => s.Isin == isin).AllocationPercentage;

            state.LastPrices[isin] = candle.Close;
            runningCandles[isin].Add(candle);

            // Trim to bounded history
            if (runningCandles[isin].Count > maxHistoryToKeep)
            {
                runningCandles[isin].RemoveRange(
                    0, runningCandles[isin].Count - maxHistoryToKeep);
            }

            // ─── 1. Execute pending fill ───
            if (state.PendingOrders.TryGetValue(isin, out var pending))
            {
                ExecuteFill(state, pending, candle, instrument, run, isin, eventLogger);
                state.PendingOrders.Remove(isin);
            }

            // ─── 2. Check SL/TP on open position ───
            if (state.Positions.TryGetValue(isin, out var pos))
            {
                var resolution = _sltpResolver.Resolve(
                    pos.StopLoss, pos.TakeProfit, candle, ev.SubCandles);

                if (resolution.Triggered)
                {
                    ClosePosition(state, isin, resolution.Price,
                        resolution.Reason, candle.OpenTime, instrument, run, eventLogger);
                }
            }

            // ─── 3. Evaluate strategy at Close (if no pending) ───
            if (!state.PendingOrders.ContainsKey(isin))
            {
                var histCandles = runningCandles[isin];
                if (histCandles.Count >= strategy.RequiredCandleCount)
                {
                    // Build ExtraTimeFrames dict for this event
                    IReadOnlyDictionary<TimeFrame, IReadOnlyList<Candle>>? extraForStrategy = null;
                    if (strategyReqs.Count > 0)
                    {
                        var windowEnd = candle.OpenTime + tfDuration;
                        var dict = new Dictionary<TimeFrame, IReadOnlyList<Candle>>();

                        foreach (var req in strategyReqs)
                        {
                            var full = extraCandleLists[(isin, req.TimeFrame)];
                            var reqDur = req.TimeFrame.ToDuration();

                            var slice = full
                                .Where(c => c.OpenTime + reqDur <= windowEnd)
                                .TakeLast(req.Count)
                                .ToList();

                            dict[req.TimeFrame] = slice;
                        }

                        extraForStrategy = dict;
                    }

                    var input = new StrategyInput(
                        Isin: isin,
                        LatestSnapshot: null,
                        Candles: histCandles,
                        ExtraTimeFrames: extraForStrategy,    // ← NEW
                        Configuration: strategyConfig,
                        TimeFrame: timeFrame,
                        EvaluatedAt: candle.OpenTime);

                    StrategyEvaluationResult evalResult;
                    try
                    {
                        evalResult = strategy.Evaluate(input);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Strategy evaluate failed at {Time} for {Isin}.",
                            candle.OpenTime, isin);
                        evalResult = StrategyEvaluationResult.Empty();
                    }

                    foreach (var sig in evalResult.Signals)
                    {
                        HandleSignal(state, sig, isin, candle, instrument,
                            allocPct, run, sizeCalc, eventLogger);
                    }
                }
            }

            // ─── 4. End-of-day equity ───
            // candle.OpenTime is UTC; convert to Tehran trading date to avoid off-by-one
            // (BC6 fix). Tehran midnight → prev day 20:30 UTC, so .Date on raw UTC
            // would attribute the day's equity to the previous day.
            var tradingDate = TehranDate.FromUtc(candle.OpenTime);
            var dayOfCandle = tradingDate.ToDateTime(TimeOnly.MinValue);
            if (currentDay != dayOfCandle)
            {
                if (currentDay.HasValue)
                {
                    RecordEquity(state, run.Id, currentDay.Value, lastEquity);
                }
                currentDay = dayOfCandle;
                lastEquity = ComputeEquity(state);
            }

            // ─── 5. Progress log (every 10%) ───
            if (eventIdx - lastProgressLog >= totalEvents / 10)
            {
                lastProgressLog = eventIdx;
                _logger.LogInformation(
                    "BacktestRun {RunId} progress: {Done:N0}/{Total:N0} ({Pct:0.0}%)",
                    run.Id, eventIdx + 1, totalEvents,
                    (eventIdx + 1) * 100.0 / totalEvents);
            }
        }

        // ─── Final day equity ───
        if (currentDay.HasValue)
            RecordEquity(state, run.Id, currentDay.Value, lastEquity);

        // ─── Close all remaining at last close ───
        foreach (var (isin, pos) in state.Positions.ToList())
        {
            var lastCandle = primaryCandles[isin].Last();
            ClosePosition(state, isin, lastCandle.Close,
                BacktestExitReason.EndOfBacktest,
                lastCandle.OpenTime, instruments[isin], run, eventLogger);
        }

        // ─── Final day equity after close ───
        if (currentDay.HasValue)
        {
            var finalEquity = ComputeEquity(state);
            // Update last equity entry in place — RunId is already correct
            if (state.DailyEquities.Count > 0)
            {
                var lastEq = state.DailyEquities[^1];
                state.DailyEquities[^1] = BacktestDailyEquity.Create(
                    backtestRunId: lastEq.BacktestRunId,
                    date: lastEq.Date,
                    equity: finalEquity,
                    cash: state.Cash,
                    positionValue: 0m,
                    dailyReturnPct: ComputeDailyReturn(finalEquity, lastEquity));
            }
        }

        return state;
    }

    /* ═══════════════════ Signal Handling ═══════════════════ */

    private void HandleSignal(
    BacktestState state,
    StrategySignal sig,
    string isin,
    Candle candle,
    Instrument instrument,
    decimal allocPct,
    BacktestRun run,
    Trader.Application.Abstractions.Execution.IOrderSizeCalculator sizeCalc,
    IBacktestEventLogger eventLogger)
    {
        if (sig.Action == SignalAction.Buy)
        {
            var targetCapital = state.InitialCapital * allocPct / 100m;

            decimal committed = 0m;
            if (state.Positions.TryGetValue(isin, out var existing))
                committed = existing.Quantity * existing.WeightedAvgEntryPrice;

            // Apply MaxPositionSize risk cap (align with live) — D55
            decimal? maxPositionValue = null;
            var maxPosOpts = _riskOptions.CurrentValue.MaxPositionSize;
            if (maxPosOpts.Enabled)
            {
                // Backtest always applies (no separate ExecutionMode)
                maxPositionValue = state.InitialCapital * (maxPosOpts.MaxPercentPerSymbol / 100m);
            }

            var result = sizeCalc.CalculateBuy(
                new Trader.Application.Abstractions.Execution.BuySizeInput(
                    AllocatedCapital: targetCapital,
                    CommittedCapital: committed,
                    AvailableCash: state.Cash,
                    SignalStrength: sig.Strength.Value,
                    StrengthMultiplier: run.StrengthMultiplier,
                    MinDeltaPercent: run.MinDeltaPercent,
                    Price: candle.Close,
                    BuyCommissionRate: instrument.BuyCommissionRate,
                    MinOrderQuantity: instrument.MinOrderQuantity,
                    MaxOrderQuantity: instrument.MaxOrderQuantity,
                    MinOrderValueRial: instrument.MinOrderValueRial,
                    MaxPositionValue: maxPositionValue));

            if (result.Quantity > 0)
            {
                state.PendingOrders[isin] = new PendingOrder(
                    isin, OrderSide.Buy, result.Quantity, candle.OpenTime);

                eventLogger.Log(run.Id, BacktestEventType.SignalGenerated, BacktestEventSeverity.Info,
                    $"سیگنال خرید {isin}: {result.Quantity:N0} سهم @ {candle.Close:N0} " +
                    $"(قدرت: {sig.Strength.Value:0.##})",
                    payload: new
                    {
                        action = "Buy",
                        strength = sig.Strength.Value,
                        quantity = result.Quantity,
                        price = candle.Close,
                        targetCapital = result.TargetCapital,
                        committedCapital = result.CommittedCapital,
                        delta = result.Delta
                    },
                    isin: isin,
                    candleTime: candle.OpenTime);
            }
        }
        else if (sig.Action == SignalAction.Sell)
        {
            if (!state.Positions.TryGetValue(isin, out var pos))
                return;

            var result = sizeCalc.CalculateSell(
                new Trader.Application.Abstractions.Execution.SellSizeInput(
                    PositionQuantity: pos.Quantity,
                    MinOrderQuantity: instrument.MinOrderQuantity,
                    MaxOrderQuantity: instrument.MaxOrderQuantity));

            if (result.Quantity > 0)
            {
                state.PendingOrders[isin] = new PendingOrder(
                    isin, OrderSide.Sell, result.Quantity, candle.OpenTime);

                eventLogger.Log(run.Id, BacktestEventType.SignalGenerated, BacktestEventSeverity.Info,
                    $"سیگنال فروش {isin}: {result.Quantity:N0} سهم @ {candle.Close:N0} " +
                    $"(قدرت: {sig.Strength.Value:0.##})",
                    payload: new
                    {
                        action = "Sell",
                        strength = sig.Strength.Value,
                        quantity = result.Quantity,
                        price = candle.Close
                    },
                    isin: isin,
                    candleTime: candle.OpenTime);
            }
        }
    }

    /* ═══════════════════ Fill / Close ═══════════════════ */

    private void ExecuteFill(
    BacktestState state,
    PendingOrder pending,
    Candle fillCandle,
    Instrument instrument,
    BacktestRun run,
    string isin,
    IBacktestEventLogger eventLogger)
    {
        var fillPrice = _fillSimulator.FillPrice(
            pending.Side, fillCandle.Open, run.SlippagePercent);

        if (fillPrice <= 0m) return;

        // Slippage: computed per-branch after quantity is known (BC5 fix)
        var expected = fillCandle.Open;

        if (pending.Side == OrderSide.Buy)
        {
            var commissionRate = instrument.BuyCommissionRate;

            var affordableQty = FillMath.CalculateMaxBuyQuantity(
                state.Cash, fillPrice, commissionRate);

            var qty = Math.Min(pending.Quantity, affordableQty);

            if (qty < instrument.MinOrderQuantity) return;

            // Slippage after clamp (BC5 fix)
            state.TotalSlippage += Math.Abs(fillPrice - expected) * qty;

            var commission = FillMath.CalculateCommission(fillPrice, qty, commissionRate);
            var totalCost = fillPrice * qty + commission;

            state.Cash -= totalCost;
            state.TotalCommission += commission;

            eventLogger.Log(run.Id, BacktestEventType.FillExecuted, BacktestEventSeverity.Info,
                $"خرید انجام شد: {qty:N0} سهم {isin} @ {fillPrice:N0}",
                payload: new
                {
                    side = "Buy",
                    quantity = qty,
                    price = fillPrice,
                    commission = commission,
                    totalCost = totalCost
                },
                isin: isin,
                candleTime: fillCandle.OpenTime);

            if (state.Positions.TryGetValue(isin, out var existingPos))
            {
                var newAvg = FillMath.CalculateWeightedAveragePrice(
                    currentQty: existingPos.Quantity,
                    currentAvgPrice: existingPos.WeightedAvgEntryPrice,
                    fillQty: qty,
                    fillPrice: fillPrice);

                existingPos.Quantity += qty;
                existingPos.WeightedAvgEntryPrice = newAvg;
                existingPos.TotalBuyCommission += commission;

                // SL/TP recomputed from new avg (BC4 fix).
                // ResolveStopLoss returns absolute price if AbsolutePrice is set,
                // so absolute-price mode is unaffected — only percentage mode re-computes.
                //existingPos.StopLoss = ResolveStopLoss(run, newAvg);
                //existingPos.TakeProfit = ResolveTakeProfit(run, newAvg);
                existingPos.Quantity += qty;
                existingPos.WeightedAvgEntryPrice = newAvg;
                existingPos.TotalBuyCommission += commission;
                // SL/TP unchanged — matches Position.ApplyBuyFill behavior
                existingPos.EntryFills.Add(new EntryFill(
                    fillCandle.OpenTime, fillPrice, qty, commission));
            }
            else
            {
                var sl = ResolveStopLoss(run, fillPrice);
                var tp = ResolveTakeProfit(run, fillPrice);

                var pos = new OpenPosition
                {
                    Isin = isin,
                    EntryTime = fillCandle.OpenTime,
                    Quantity = qty,
                    WeightedAvgEntryPrice = fillPrice,
                    TotalBuyCommission = commission,
                    StopLoss = sl,
                    TakeProfit = tp
                };
                pos.EntryFills.Add(new EntryFill(
                    fillCandle.OpenTime, fillPrice, qty, commission));
                state.Positions[isin] = pos;

                eventLogger.Log(run.Id, BacktestEventType.PositionOpened, BacktestEventSeverity.Info,
                    $"موقعیت باز شد: {qty:N0} سهم {isin} @ {fillPrice:N0}" +
                    (sl.HasValue ? $" (SL: {sl.Value:N0})" : "") +
                    (tp.HasValue ? $" (TP: {tp.Value:N0})" : ""),
                    payload: new
                    {
                        quantity = qty,
                        entryPrice = fillPrice,
                        stopLoss = sl,
                        takeProfit = tp,
                        commission = commission
                    },
                    isin: isin,
                    candleTime: fillCandle.OpenTime);
            }
        }
        else // Sell
        {
            if (!state.Positions.TryGetValue(isin, out var pos)) return;

            var sellQty = Math.Min(pending.Quantity, pos.Quantity);
            if (sellQty <= 0) return;

            // Slippage after clamp (BC5 fix)
            state.TotalSlippage += Math.Abs(fillPrice - expected) * sellQty;

            var commissionRate = instrument.SellCommissionRate;
            var commission = FillMath.CalculateCommission(fillPrice, sellQty, commissionRate);
            var proceeds = fillPrice * sellQty - commission;

            state.Cash += proceeds;
            state.TotalCommission += commission;

            // Proportional share of buy commission for this partial exit (BC1/BC2 fix)
            var proportionalBuyCommission = pos.Quantity > 0
                ? pos.TotalBuyCommission * sellQty / pos.Quantity
                : 0m;

            var entryFillsJson = JsonSerializer.Serialize(
                pos.EntryFills,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Trade captures ONLY the slice being closed — not the entire position
            var trade = Domain.Entities.BacktestTrade.Create(
                backtestRunId: run.Id,
                isin: isin,
                entryTime: pos.EntryTime,
                lastEntryTime: pos.EntryFills[^1].Time,
                totalEntryQuantity: sellQty,                       // ← BC1 fix
                weightedAvgEntryPrice: pos.WeightedAvgEntryPrice,
                totalBuyCommission: proportionalBuyCommission,     // ← BC1/BC2 fix
                entryFillsJson: entryFillsJson,
                exitTime: fillCandle.OpenTime,
                exitPrice: fillPrice,
                exitQuantity: sellQty,
                totalSellCommission: commission,
                exitReason: BacktestExitReason.Signal);

            eventLogger.Log(run.Id, BacktestEventType.PositionClosed, BacktestEventSeverity.Info,
                $"موقعیت بسته شد (سیگنال): {sellQty:N0} سهم {isin} @ {fillPrice:N0}",
                payload: new
                {
                    quantity = sellQty,
                    exitPrice = fillPrice,
                    commission = commission,
                    exitReason = "Signal",
                    isPartial = sellQty < pos.Quantity
                },
                isin: isin,
                candleTime: fillCandle.OpenTime,
                tradeId: trade.Id);

            state.Trades.Add(trade);

            // Only remove when fully sold — otherwise reduce in place (BC1 fix)
            if (sellQty >= pos.Quantity)
            {
                state.Positions.Remove(isin);
            }
            else
            {
                pos.Quantity -= sellQty;
                pos.TotalBuyCommission -= proportionalBuyCommission;
            }
        }
    }

    private void ClosePosition(
    BacktestState state,
    string isin,
    decimal triggerPrice,
    BacktestExitReason reason,
    DateTime time,
    Instrument instrument,
    BacktestRun run,
    IBacktestEventLogger eventLogger)
    {
        if (!state.Positions.TryGetValue(isin, out var pos)) return;

        // SL/TP fill با slippage
        var fillPrice = _fillSimulator.FillPrice(
            OrderSide.Sell, triggerPrice, run.SlippagePercent);

        state.TotalSlippage += Math.Abs(fillPrice - triggerPrice) * pos.Quantity;

        var commissionRate = instrument.SellCommissionRate;
        var commission = FillMath.CalculateCommission(fillPrice, pos.Quantity, commissionRate);
        var proceeds = fillPrice * pos.Quantity - commission;

        state.Cash += proceeds;
        state.TotalCommission += commission;

        var entryFillsJson = JsonSerializer.Serialize(
            pos.EntryFills,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var trade = Domain.Entities.BacktestTrade.Create(
            backtestRunId: run.Id,
            isin: isin,
            entryTime: pos.EntryTime,
            lastEntryTime: pos.EntryFills[^1].Time,
            totalEntryQuantity: pos.Quantity,
            weightedAvgEntryPrice: pos.WeightedAvgEntryPrice,
            totalBuyCommission: pos.TotalBuyCommission,
            entryFillsJson: entryFillsJson,
            exitTime: time,
            exitPrice: fillPrice,
            exitQuantity: pos.Quantity,
            totalSellCommission: commission,
            exitReason: reason);

        state.Trades.Add(trade);
        state.Positions.Remove(isin);
        if (reason == BacktestExitReason.StopLoss || reason == BacktestExitReason.TakeProfit)
        {
            eventLogger.Log(run.Id,
                BacktestEventType.SLTPTriggered,
                reason == BacktestExitReason.StopLoss
                    ? BacktestEventSeverity.Warning
                    : BacktestEventSeverity.Info,
                $"{(reason == BacktestExitReason.StopLoss ? "حد ضرر" : "حد سود")} فعال شد: " +
                $"{pos.Quantity:N0} سهم {isin} @ {fillPrice:N0} (trigger: {triggerPrice:N0})",
                payload: new
                {
                    reason = reason.ToString(),
                    quantity = pos.Quantity,
                    triggerPrice = triggerPrice,
                    fillPrice = fillPrice,
                    commission = commission
                },
                isin: isin,
                candleTime: time,
                tradeId: trade.Id);
        }
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static decimal? ResolveStopLoss(BacktestRun run, decimal entryPrice)
    {
        if (run.StopLossPrice.HasValue)
            return run.StopLossPrice.Value;
        if (run.StopLossPercentage.HasValue)
            return StopLossConfig.FromPercentage(run.StopLossPercentage.Value)
                .ResolveForEntry(new Price(entryPrice)).Value;
        return null;
    }

    private static decimal? ResolveTakeProfit(BacktestRun run, decimal entryPrice)
    {
        if (run.TakeProfitPrice.HasValue)
            return run.TakeProfitPrice.Value;
        if (run.TakeProfitPercentage.HasValue)
            return TakeProfitConfig.FromPercentage(run.TakeProfitPercentage.Value)
                .ResolveForEntry(new Price(entryPrice)).Value;
        return null;
    }

    private static decimal ComputeEquity(BacktestState state)
    {
        var posValue = 0m;
        foreach (var (isin, pos) in state.Positions)
        {
            if (state.LastPrices.TryGetValue(isin, out var lastPrice))
                posValue += pos.Quantity * lastPrice;
        }
        return state.Cash + posValue;
    }

    private static void RecordEquity(
    BacktestState state,
    Guid runId,
    DateTime day,
    decimal prevDayEquity)
    {
        var equity = ComputeEquity(state);
        var posValue = equity - state.Cash;

        var dailyReturn = prevDayEquity > 0m
            ? (equity - prevDayEquity) / prevDayEquity * 100m
            : 0m;

        state.DailyEquities.Add(Domain.Entities.BacktestDailyEquity.Create(
            backtestRunId: runId,
            date: DateTime.SpecifyKind(day, DateTimeKind.Utc),
            equity: equity,
            cash: state.Cash,
            positionValue: posValue,
            dailyReturnPct: Math.Round(dailyReturn, 4)));
    }

    private static decimal ComputeDailyReturn(decimal current, decimal prev)
        => prev > 0m ? (current - prev) / prev * 100m : 0m;

    private async Task<List<Candle>> LoadCandlesAsync(
    ITraderUnitOfWork<TraderDbContext> uow,
    string isin,
    TimeFrame timeFrame,
    DateTime from,
    DateTime to,
    CancellationToken ct)
    {
        // Read-only loading — bypass tracking for large historical sets
        // (60k+ rows with tracking → 30s+ timeout)
        var rows = await uow.Context.Candles
            .AsNoTracking()
            .Where(c => c.Isin == isin
                    && (int)c.TimeFrame == (int)timeFrame
                     && c.OpenTime >= from
                     && c.OpenTime <= to)
            .OrderBy(c => c.OpenTime)
            .ToListAsync(ct);

        return rows
            .Select(r => new Candle(
                OpenTime: r.OpenTime,
                Open: r.Open,
                High: r.High,
                Low: r.Low,
                Close: r.Close,
                Volume: r.Volume))
            .ToList();
    }
}