using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Backtests;
using Trader.Application.Specifications.Candles;
using Trader.Application.Specifications.Instruments;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Backtesting;

public sealed class BacktestEngine : IBacktestEngine
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISubCandleResolver _sltpResolver;
    private readonly IOrderFillSimulator _fillSimulator;
    private readonly ILogger<BacktestEngine> _logger;

    public BacktestEngine(
        IServiceScopeFactory scopeFactory,
        ISubCandleResolver sltpResolver,
        IOrderFillSimulator fillSimulator,
        ILogger<BacktestEngine> logger)
    {
        _scopeFactory = scopeFactory;
        _sltpResolver = sltpResolver;
        _fillSimulator = fillSimulator;
        _logger = logger;
    }

    public async Task RunAsync(Guid backtestRunId, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

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

        try
        {
            var state = await SimulateAsync(run, uow, ct);

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
        }
        catch (OperationCanceledException)
        {
            run.MarkCancelled();
            await uow.BacktestRuns.UpdateAsync(run);
            await uow.SaveChangesAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BacktestRun {Id} failed.", run.Id);
            run.MarkFailed(ex.Message);
            await uow.BacktestRuns.UpdateAsync(run);
            await uow.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    /* ═══════════════════ Simulation ═══════════════════ */

    private async Task<BacktestState> SimulateAsync(
        BacktestRun run,
        ITraderUnitOfWork<TraderDbContext> uow,
        CancellationToken ct)
    {
        var scope = _scopeFactory.CreateScope().ServiceProvider;

        var strategyFactory = scope.GetRequiredService<IStrategyFactory>();
        var sizeCalc = scope.GetRequiredService<Trader.Application.Abstractions.Execution.IOrderSizeCalculator>();
        var transformer = scope.GetRequiredService<ICandleTransformer>();

        // ─── Config ───
        var strategyConfig = strategyFactory.DeserializeConfig(
            run.StrategyKey, run.StrategyConfigJson);
        var strategy = strategyFactory.Create(run.StrategyKey, strategyConfig);

        var timeFrame = (TimeFrame)run.PrimaryTimeFrame;
        var subTimeFrame = run.SubCandleTimeFrame.HasValue
            ? (TimeFrame)run.SubCandleTimeFrame.Value
            : (TimeFrame?)null;

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

        // ─── State ───
        var state = new BacktestState
        {
            InitialCapital = run.InitialCapital,
            Cash = run.InitialCapital
        };

        // ─── Build Events ───
        var events = new List<BacktestEvent>();
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

                events.Add(new BacktestEvent(
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
                ExecuteFill(state, pending, candle, instrument, run, isin);
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
                        resolution.Reason, candle.OpenTime, instrument, run);
                }
            }

            // ─── 3. Evaluate strategy at Close (if no pending) ───
            if (!state.PendingOrders.ContainsKey(isin))
            {
                var histCandles = runningCandles[isin];
                if (histCandles.Count >= strategy.RequiredCandleCount)
                {
                    var input = new StrategyInput(
                        Isin: isin,
                        LatestSnapshot: null,
                        Candles: histCandles,
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
                            allocPct, run, sizeCalc);
                    }
                }
            }

            // ─── 4. End-of-day equity ───
            var dayOfCandle = candle.OpenTime.Date;
            if (currentDay != dayOfCandle)
            {
                if (currentDay.HasValue)
                {
                    RecordEquity(state, currentDay.Value, lastEquity);
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
            RecordEquity(state, currentDay.Value, lastEquity);

        // ─── Close all remaining at last close ───
        foreach (var (isin, pos) in state.Positions.ToList())
        {
            var lastCandle = primaryCandles[isin].Last();
            ClosePosition(state, isin, lastCandle.Close,
                BacktestExitReason.EndOfBacktest,
                lastCandle.OpenTime, instruments[isin], run);
        }

        // ─── Final day equity after close ───
        if (currentDay.HasValue)
        {
            var finalEquity = ComputeEquity(state);
            // Update last equity entry
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

        // Fix backtestRunId on daily equities (they were created with Guid.Empty)
        for (int i = 0; i < state.DailyEquities.Count; i++)
        {
            var eq = state.DailyEquities[i];
            state.DailyEquities[i] = BacktestDailyEquity.Create(
                backtestRunId: run.Id,
                date: eq.Date,
                equity: eq.Equity,
                cash: eq.Cash,
                positionValue: eq.PositionValue,
                dailyReturnPct: eq.DailyReturnPct);
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
        Trader.Application.Abstractions.Execution.IOrderSizeCalculator sizeCalc)
    {
        if (sig.Action == SignalAction.Buy)
        {
            var targetCapital = state.InitialCapital * allocPct / 100m;

            decimal committed = 0m;
            if (state.Positions.TryGetValue(isin, out var existing))
                committed = existing.Quantity * existing.WeightedAvgEntryPrice;

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
                    MinOrderValueRial: instrument.MinOrderValueRial));

            if (result.Quantity > 0)
            {
                state.PendingOrders[isin] = new PendingOrder(
                    isin, OrderSide.Buy, result.Quantity, candle.OpenTime);
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
        string isin)
    {
        var fillPrice = _fillSimulator.FillPrice(
            pending.Side, fillCandle.Open, run.SlippagePercent);

        if (fillPrice <= 0m) return;

        // slippage amount
        var expected = pending.Side == OrderSide.Buy
            ? fillCandle.Open
            : fillCandle.Open;
        state.TotalSlippage += Math.Abs(fillPrice - expected) * pending.Quantity;

        if (pending.Side == OrderSide.Buy)
        {
            // ─── Re-check cash ───
            var commissionRate = instrument.BuyCommissionRate;
            var cost = fillPrice * pending.Quantity * (1m + commissionRate);
            var qty = pending.Quantity;

            if (cost > state.Cash)
            {
                var maxQty = (long)Math.Floor(state.Cash / (fillPrice * (1m + commissionRate)));
                if (maxQty < instrument.MinOrderQuantity) return;
                qty = maxQty;
            }

            if (qty < instrument.MinOrderQuantity) return;

            var commission = fillPrice * qty * commissionRate;
            var totalCost = fillPrice * qty + commission;

            if (totalCost > state.Cash) return;

            state.Cash -= totalCost;
            state.TotalCommission += commission;

            if (state.Positions.TryGetValue(isin, out var existingPos))
            {
                var newQty = existingPos.Quantity + qty;
                var newAvg = (existingPos.Quantity * existingPos.WeightedAvgEntryPrice
                    + qty * fillPrice) / newQty;
                existingPos.Quantity = newQty;
                existingPos.WeightedAvgEntryPrice = newAvg;
                existingPos.TotalBuyCommission += commission;
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
            }
        }
        else // Sell
        {
            if (!state.Positions.TryGetValue(isin, out var pos)) return;
            var sellQty = Math.Min(pending.Quantity, pos.Quantity);
            if (sellQty <= 0) return;

            var commissionRate = instrument.SellCommissionRate;
            var proceeds = fillPrice * sellQty * (1m - commissionRate);
            var commission = fillPrice * sellQty * commissionRate;

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
                exitTime: fillCandle.OpenTime,
                exitPrice: fillPrice,
                exitQuantity: sellQty,
                totalSellCommission: commission,
                exitReason: BacktestExitReason.Signal);

            state.Trades.Add(trade);
            state.Positions.Remove(isin);
        }
    }

    private void ClosePosition(
        BacktestState state,
        string isin,
        decimal triggerPrice,
        BacktestExitReason reason,
        DateTime time,
        Instrument instrument,
        BacktestRun run)
    {
        if (!state.Positions.TryGetValue(isin, out var pos)) return;

        // SL/TP fill با slippage
        var fillPrice = _fillSimulator.FillPrice(
            OrderSide.Sell, triggerPrice, run.SlippagePercent);

        state.TotalSlippage += Math.Abs(fillPrice - triggerPrice) * pos.Quantity;

        var commissionRate = instrument.SellCommissionRate;
        var proceeds = fillPrice * pos.Quantity * (1m - commissionRate);
        var commission = fillPrice * pos.Quantity * commissionRate;

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
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static decimal? ResolveStopLoss(BacktestRun run, decimal entryPrice)
    {
        if (run.StopLossPrice.HasValue) return run.StopLossPrice.Value;
        if (run.StopLossPercentage.HasValue)
            return entryPrice * (1m - run.StopLossPercentage.Value / 100m);
        return null;
    }

    private static decimal? ResolveTakeProfit(BacktestRun run, decimal entryPrice)
    {
        if (run.TakeProfitPrice.HasValue) return run.TakeProfitPrice.Value;
        if (run.TakeProfitPercentage.HasValue)
            return entryPrice * (1m + run.TakeProfitPercentage.Value / 100m);
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
        DateTime day,
        decimal prevDayEquity)
    {
        var equity = ComputeEquity(state);
        var posValue = equity - state.Cash;

        var dailyReturn = prevDayEquity > 0m
            ? (equity - prevDayEquity) / prevDayEquity * 100m
            : 0m;

        state.DailyEquities.Add(Domain.Entities.BacktestDailyEquity.Create(
            backtestRunId: Guid.Empty,   // set by caller
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
                     && c.TimeFrame == (int)timeFrame
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