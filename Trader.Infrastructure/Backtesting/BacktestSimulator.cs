using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Runtime;
using Trader.Application.Strategies;
using Trader.Domain.Calculations;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Infrastructure.Backtesting;

public sealed class BacktestSimulator : IBacktestSimulator
{
    private readonly IBacktestDataCache _cache;
    private readonly IStrategyFactory _strategyFactory;
    private readonly IOrderSizeCalculator _sizeCalc;
    private readonly ICandleTransformer _transformer;
    private readonly ISubCandleResolver _sltpResolver;
    private readonly IOrderFillSimulator _fillSimulator;
    private readonly IOptionsMonitor<TradingRiskOptions> _riskOptions;
    private readonly ILogger<BacktestSimulator> _logger;

    public BacktestSimulator(
        IBacktestDataCache cache,
        IStrategyFactory strategyFactory,
        IOrderSizeCalculator sizeCalc,
        ICandleTransformer transformer,
        ISubCandleResolver sltpResolver,
        IOrderFillSimulator fillSimulator,
        IOptionsMonitor<TradingRiskOptions> riskOptions,
        ILogger<BacktestSimulator> logger)
    {
        _cache = cache;
        _strategyFactory = strategyFactory;
        _sizeCalc = sizeCalc;
        _transformer = transformer;
        _sltpResolver = sltpResolver;
        _fillSimulator = fillSimulator;
        _riskOptions = riskOptions;
        _logger = logger;
    }

    public async Task<BacktestSimulationResult> SimulateAsync(
        BacktestSimulationRequest request,
        IBacktestEventLogger eventLogger,
        CancellationToken ct = default)
    {
        // ─── Config ───
        var strategyConfig = _strategyFactory.DeserializeConfig(
            request.StrategyKey, request.StrategyConfigJson);
        var strategy = _strategyFactory.Create(request.StrategyKey, strategyConfig);

        var timeFrame = request.PrimaryTimeFrame;
        var subTimeFrame = request.SubCandleTimeFrame;

        if (subTimeFrame.HasValue && (int)subTimeFrame.Value >= (int)timeFrame)
            subTimeFrame = null;

        var runId = Guid.Empty; // tuning: not persisted

        eventLogger.Log(runId, BacktestEventType.ConfigLoaded, BacktestEventSeverity.Info,
            $"Config لود شد. استراتژی: {request.StrategyKey}, تایم‌فریم: {timeFrame}, " +
            $"sub-candle: {subTimeFrame?.ToString() ?? "غیرفعال"}, " +
            $"Strength: {request.StrengthMultiplier}, MinDelta: {request.MinDeltaPercent:P1}",
            payload: new { strategyKey = request.StrategyKey });

        // ─── Instruments (via cache) ───
        var instruments = new Dictionary<string, Instrument>();
        foreach (var s in request.Symbols)
            instruments[s.Isin] = await _cache.GetInstrumentAsync(s.Isin, ct);

        // ─── Candles — TWO STREAMS ───
        var primaryCandlesIrr = new Dictionary<string, List<Candle>>();
        var primaryCandlesStrategy = new Dictionary<string, List<Candle>>();
        var subCandles = subTimeFrame.HasValue
            ? new Dictionary<string, List<Candle>>() : null;

        var needsTransform =
            !string.IsNullOrWhiteSpace(request.TargetCurrency)
            && request.TargetCurrency != "IRR";

        foreach (var s in request.Symbols)
        {
            var primaryIrr = (await _cache.GetCandlesAsync(
                s.Isin, timeFrame, request.DateFrom, request.DateTo, ct)).ToList();

            if (primaryIrr.Count < strategy.RequiredCandleCount + 1)
                throw new InvalidOperationException(
                    $"Not enough candles for {s.Isin}: have {primaryIrr.Count}, " +
                    $"need {strategy.RequiredCandleCount + 1}.");

            List<Candle> primaryStrategy = primaryIrr;
            if (needsTransform)
            {
                primaryStrategy = (await _cache.GetTransformedCandlesAsync(
                    s.Isin, timeFrame, request.DateFrom, request.DateTo,
                    request.TargetCurrency!, ct)).ToList();

                if (primaryStrategy.Count != primaryIrr.Count)
                    throw new InvalidOperationException(
                        $"Currency transform count mismatch for {s.Isin}: " +
                        $"IRR={primaryIrr.Count} vs Strategy={primaryStrategy.Count}.");
            }

            primaryCandlesIrr[s.Isin] = primaryIrr;
            primaryCandlesStrategy[s.Isin] = primaryStrategy;

            eventLogger.Log(runId, BacktestEventType.CandlesLoaded, BacktestEventSeverity.Info,
                $"کندل‌های {s.Isin} لود شدند: {primaryIrr.Count} کندل",
                payload: new { isin = s.Isin, count = primaryIrr.Count },
                isin: s.Isin);

            if (subCandles is not null)
            {
                subCandles[s.Isin] = (await _cache.GetCandlesAsync(
                    s.Isin, subTimeFrame!.Value, request.DateFrom, request.DateTo, ct))
                    .ToList();
            }
        }

        // ─── Extra TimeFrames (Strategy reqs + sub) ───
        var strategyReqs = strategy.ExtraCandleRequirements
            .Where(r => r.TimeFrame != timeFrame)
            .ToList();

        var neededExtraTfs = new HashSet<TimeFrame>();
        if (subTimeFrame.HasValue) neededExtraTfs.Add(subTimeFrame.Value);
        foreach (var req in strategyReqs) neededExtraTfs.Add(req.TimeFrame);

        var extraCandleLists = new Dictionary<(string Isin, TimeFrame Tf), List<Candle>>();
        foreach (var s in request.Symbols)
        {
            foreach (var tf in neededExtraTfs)
            {
                List<Candle> extraIrr;
                if (subCandles is not null
                    && subTimeFrame == tf
                    && subCandles.TryGetValue(s.Isin, out var existing)
                    && existing.Count > 0)
                {
                    extraIrr = existing;
                }
                else
                {
                    extraIrr = (await _cache.GetCandlesAsync(
                        s.Isin, tf, request.DateFrom, request.DateTo, ct)).ToList();
                }

                List<Candle> extra = extraIrr;
                if (needsTransform && extraIrr.Count > 0)
                {
                    extra = (await _cache.GetTransformedCandlesAsync(
                        s.Isin, tf, request.DateFrom, request.DateTo,
                        request.TargetCurrency!, ct)).ToList();
                }

                extraCandleLists[(s.Isin, tf)] = extra;
            }
        }

        // ─── State ───
        var state = new BacktestState
        {
            InitialCapital = request.InitialCapital,
            Cash = request.InitialCapital,
        };

        // ─── Build Events ───
        var events = new List<BacktestCandleEvent>();
        var tfDuration = timeFrame.ToDuration();

        foreach (var s in request.Symbols)
        {
            var primIrr = primaryCandlesIrr[s.Isin];
            var primStrategy = primaryCandlesStrategy[s.Isin];

            for (int i = 0; i < primIrr.Count; i++)
            {
                var cIrr = primIrr[i];
                if (cIrr.OpenTime < request.DateFrom || cIrr.OpenTime > request.DateTo)
                    continue;

                Candle? cStrategy = needsTransform && i < primStrategy.Count
                    ? primStrategy[i] : null;

                IReadOnlyList<Candle>? subs = null;
                if (subCandles is not null && subCandles.ContainsKey(s.Isin))
                {
                    var end = cIrr.OpenTime + tfDuration;
                    subs = subCandles[s.Isin]
                        .Where(sc => sc.OpenTime >= cIrr.OpenTime && sc.OpenTime < end)
                        .ToList();
                }

                events.Add(new BacktestCandleEvent(
                    Time: cIrr.OpenTime, Isin: s.Isin,
                    Candle: cIrr, StrategyCandle: cStrategy, SubCandles: subs));
            }
        }

        events.Sort((a, b) =>
        {
            var cmp = a.Time.CompareTo(b.Time);
            return cmp != 0 ? cmp : string.Compare(a.Isin, b.Isin, StringComparison.Ordinal);
        });

        eventLogger.Log(runId, BacktestEventType.EventsBuilt, BacktestEventSeverity.Info,
            $"{events.Count:N0} رویداد شبیه‌سازی ساخته شد.",
            payload: new { eventCount = events.Count });

        eventLogger.Log(runId, BacktestEventType.SimulationStarted, BacktestEventSeverity.Info,
            "شبیه‌سازی شروع شد.");

        // ─── Running candles / daily equity ───
        var runningCandles = request.Symbols.ToDictionary(s => s.Isin, _ => new List<Candle>());
        DateTime? currentDay = null;
        decimal lastEquity = state.Cash;
        var maxHistoryToKeep = strategy.RequiredCandleCount + 20;

        // ─── Simulation Loop ───
        for (int eventIdx = 0; eventIdx < events.Count; eventIdx++)
        {
            ct.ThrowIfCancellationRequested();
            var ev = events[eventIdx];
            var isin = ev.Isin;
            var candleIrr = ev.Candle;
            var candleStrategy = ev.StrategyCandle ?? ev.Candle;
            var instrument = instruments[isin];
            var allocPct = request.Symbols.First(s => s.Isin == isin).AllocationPercentage;

            state.LastPrices[isin] = candleIrr.Close;
            runningCandles[isin].Add(candleStrategy);

            if (runningCandles[isin].Count > maxHistoryToKeep)
                runningCandles[isin].RemoveRange(
                    0, runningCandles[isin].Count - maxHistoryToKeep);

            // 1. Pending fill
            if (state.PendingOrders.TryGetValue(isin, out var pending))
            {
                ExecuteFill(state, pending, candleIrr, instrument, request, runId, eventLogger);
                state.PendingOrders.Remove(isin);
            }

            // 2. SL/TP
            if (state.Positions.TryGetValue(isin, out var pos))
            {
                var res = _sltpResolver.Resolve(
                    pos.StopLoss, pos.TakeProfit, candleIrr, ev.SubCandles);
                if (res.Triggered)
                {
                    ClosePosition(state, isin, res.Price, res.Reason,
                        candleIrr.OpenTime, instrument, request, runId, eventLogger);
                }
            }

            // 3. Strategy evaluate
            if (!state.PendingOrders.ContainsKey(isin))
            {
                var hist = runningCandles[isin];
                if (hist.Count >= strategy.RequiredCandleCount)
                {
                    IReadOnlyDictionary<TimeFrame, IReadOnlyList<Candle>>? extraForStrategy = null;
                    if (strategyReqs.Count > 0)
                    {
                        var windowEnd = candleIrr.OpenTime + tfDuration;
                        var dict = new Dictionary<TimeFrame, IReadOnlyList<Candle>>();
                        foreach (var req in strategyReqs)
                        {
                            var full = extraCandleLists[(isin, req.TimeFrame)];
                            var reqDur = req.TimeFrame.ToDuration();
                            dict[req.TimeFrame] = full
                                .Where(c => c.OpenTime + reqDur <= windowEnd)
                                .TakeLast(req.Count)
                                .ToList();
                        }
                        extraForStrategy = dict;
                    }

                    var input = new StrategyInput(
                        Isin: isin, LatestSnapshot: null,
                        Candles: hist, ExtraTimeFrames: extraForStrategy,
                        Configuration: strategyConfig,
                        TimeFrame: timeFrame,
                        EvaluatedAt: candleIrr.OpenTime);

                    StrategyEvaluationResult evalResult;
                    try { evalResult = strategy.Evaluate(input); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Strategy evaluate failed at {Time} for {Isin}.",
                            candleIrr.OpenTime, isin);
                        evalResult = StrategyEvaluationResult.Empty();
                    }

                    foreach (var sig in evalResult.Signals)
                    {
                        // D57: scale SL/TP from USD (strategy) to IRR (fill)
                        var sigForFill = needsTransform
                            ? SignalCurrencyHelper.ScaleSlTpToIrr(
                                sig, candleStrategy.Close, candleIrr.Close)
                            : sig;

                        HandleSignal(state, sigForFill, isin, candleIrr, instrument,
                            allocPct, request, runId, eventLogger);
                    }
                }
            }

            // 4. End-of-day equity
            var tradingDate = TehranDate.FromUtc(candleIrr.OpenTime);
            var dayOfCandle = tradingDate.ToDateTime(TimeOnly.MinValue);
            if (currentDay != dayOfCandle)
            {
                if (currentDay.HasValue)
                    RecordEquity(state, runId, currentDay.Value, lastEquity);
                currentDay = dayOfCandle;
                lastEquity = ComputeEquity(state);
            }
        }

        // Final day equity
        if (currentDay.HasValue)
            RecordEquity(state, runId, currentDay.Value, lastEquity);

        // Close remaining positions at last IRR close
        foreach (var (isin, pos) in state.Positions.ToList())
        {
            var lastCandle = primaryCandlesIrr[isin].Last();
            ClosePosition(state, isin, lastCandle.Close,
                BacktestExitReason.EndOfBacktest, lastCandle.OpenTime,
                instruments[isin], request, runId, eventLogger);
        }

        // Final daily equity after close
        if (currentDay.HasValue && state.DailyEquities.Count > 0)
        {
            var finalEquity = ComputeEquity(state);
            var lastEq = state.DailyEquities[^1];
            state.DailyEquities[^1] = BacktestDailyEquity.Create(
                backtestRunId: lastEq.BacktestRunId,
                date: lastEq.Date,
                equity: finalEquity,
                cash: state.Cash,
                positionValue: 0m,
                dailyReturnPct: ComputeDailyReturn(finalEquity, lastEquity));
        }

        // ─── Metrics ───
        var metrics = BacktestMetricsCalculator.Calculate(
            state.InitialCapital, state.Cash, state.Trades, state.DailyEquities);

        return new BacktestSimulationResult(
            InitialCapital: state.InitialCapital,
            FinalCash: state.Cash,
            Trades: state.Trades,
            DailyEquities: state.DailyEquities,
            TotalCommission: state.TotalCommission,
            TotalSlippage: state.TotalSlippage,
            Metrics: metrics);
    }

    /* ═══════════════════ HandleSignal ═══════════════════ */

    private void HandleSignal(
        BacktestState state, StrategySignal sig, string isin, Candle candle,
        Instrument instrument, decimal allocPct, BacktestSimulationRequest request,
        Guid runId, IBacktestEventLogger eventLogger)
    {
        if (sig.Action == SignalAction.Buy)
        {
            var targetCapital = state.InitialCapital * allocPct / 100m;
            decimal committed = 0m;
            if (state.Positions.TryGetValue(isin, out var existing))
                committed = existing.Quantity * existing.WeightedAvgEntryPrice;

            decimal? maxPositionValue = null;
            var maxPosOpts = _riskOptions.CurrentValue.MaxPositionSize;
            if (maxPosOpts.Enabled)
                maxPositionValue = state.InitialCapital * (maxPosOpts.MaxPercentPerSymbol / 100m);

            var result = _sizeCalc.CalculateBuy(new BuySizeInput(
                AllocatedCapital: targetCapital,
                CommittedCapital: committed,
                AvailableCash: state.Cash,
                SignalStrength: sig.Strength.Value,
                StrengthMultiplier: request.StrengthMultiplier,
                MinDeltaPercent: request.MinDeltaPercent,
                Price: candle.Close,
                BuyCommissionRate: instrument.BuyCommissionRate,
                MinOrderQuantity: instrument.MinOrderQuantity,
                MaxOrderQuantity: instrument.MaxOrderQuantity,
                MinOrderValueRial: instrument.MinOrderValueRial,
                MaxPositionValue: maxPositionValue));

            if (result.Quantity > 0)
            {
                state.PendingOrders[isin] = new PendingOrder(
                    isin, OrderSide.Buy, result.Quantity, candle.OpenTime,
                    StopLoss: sig.StopLoss, TakeProfit: sig.TakeProfit);

                eventLogger.Log(runId, BacktestEventType.SignalGenerated,
                    BacktestEventSeverity.Info,
                    $"سیگنال خرید {isin}: {result.Quantity:N0} سهم @ {candle.Close:N0}",
                    isin: isin, candleTime: candle.OpenTime);
            }
        }
        else if (sig.Action == SignalAction.Sell)
        {
            if (!state.Positions.TryGetValue(isin, out var pos)) return;

            var result = _sizeCalc.CalculateSell(new SellSizeInput(
                PositionQuantity: pos.Quantity,
                MinOrderQuantity: instrument.MinOrderQuantity,
                MaxOrderQuantity: instrument.MaxOrderQuantity));

            if (result.Quantity > 0)
            {
                state.PendingOrders[isin] = new PendingOrder(
                    isin, OrderSide.Sell, result.Quantity, candle.OpenTime);

                eventLogger.Log(runId, BacktestEventType.SignalGenerated,
                    BacktestEventSeverity.Info,
                    $"سیگنال فروش {isin}: {result.Quantity:N0} سهم @ {candle.Close:N0}",
                    isin: isin, candleTime: candle.OpenTime);
            }
        }
    }

    /* ═══════════════════ ExecuteFill ═══════════════════ */

    private void ExecuteFill(
        BacktestState state, PendingOrder pending, Candle fillCandle,
        Instrument instrument, BacktestSimulationRequest request,
        Guid runId, IBacktestEventLogger eventLogger)
    {
        var fillPrice = _fillSimulator.FillPrice(
            pending.Side, fillCandle.Open, request.SlippagePercent);

        if (fillPrice <= 0m) return;
        var expected = fillCandle.Open;

        if (pending.Side == OrderSide.Buy)
        {
            var commissionRate = instrument.BuyCommissionRate;
            var affordableQty = FillMath.CalculateMaxBuyQuantity(
                state.Cash, fillPrice, commissionRate);
            var qty = Math.Min(pending.Quantity, affordableQty);

            if (qty < instrument.MinOrderQuantity) return;

            state.TotalSlippage += Math.Abs(fillPrice - expected) * qty;
            var commission = FillMath.CalculateCommission(fillPrice, qty, commissionRate);
            var totalCost = fillPrice * qty + commission;
            state.Cash -= totalCost;
            state.TotalCommission += commission;

            eventLogger.Log(runId, BacktestEventType.FillExecuted,
                BacktestEventSeverity.Info,
                $"خرید انجام شد: {qty:N0} سهم {pending.Isin} @ {fillPrice:N0}",
                isin: pending.Isin, candleTime: fillCandle.OpenTime);

            if (state.Positions.TryGetValue(pending.Isin, out var existingPos))
            {
                var newAvg = FillMath.CalculateWeightedAveragePrice(
                    currentQty: existingPos.Quantity,
                    currentAvgPrice: existingPos.WeightedAvgEntryPrice,
                    fillQty: qty, fillPrice: fillPrice);

                existingPos.Quantity += qty;
                existingPos.WeightedAvgEntryPrice = newAvg;
                existingPos.TotalBuyCommission += commission;
                existingPos.EntryFills.Add(new EntryFill(
                    fillCandle.OpenTime, fillPrice, qty, commission));
            }
            else
            {
                var sl = ResolveStopLoss(request, fillPrice, pending.StopLoss);
                var tp = ResolveTakeProfit(request, fillPrice, pending.TakeProfit);

                var pos = new OpenPosition
                {
                    Isin = pending.Isin,
                    EntryTime = fillCandle.OpenTime,
                    Quantity = qty,
                    WeightedAvgEntryPrice = fillPrice,
                    TotalBuyCommission = commission,
                    StopLoss = sl,
                    TakeProfit = tp,
                };
                pos.EntryFills.Add(new EntryFill(
                    fillCandle.OpenTime, fillPrice, qty, commission));
                state.Positions[pending.Isin] = pos;

                eventLogger.Log(runId, BacktestEventType.PositionOpened,
                    BacktestEventSeverity.Info,
                    $"موقعیت باز شد: {qty:N0} سهم {pending.Isin} @ {fillPrice:N0}",
                    isin: pending.Isin, candleTime: fillCandle.OpenTime);
            }
        }
        else // Sell
        {
            if (!state.Positions.TryGetValue(pending.Isin, out var pos)) return;
            var sellQty = Math.Min(pending.Quantity, pos.Quantity);
            if (sellQty <= 0) return;

            state.TotalSlippage += Math.Abs(fillPrice - expected) * sellQty;
            var commissionRate = instrument.SellCommissionRate;
            var commission = FillMath.CalculateCommission(fillPrice, sellQty, commissionRate);
            var proceeds = fillPrice * sellQty - commission;
            state.Cash += proceeds;
            state.TotalCommission += commission;

            var propBuyComm = pos.Quantity > 0
                ? pos.TotalBuyCommission * sellQty / pos.Quantity : 0m;

            var entryFillsJson = JsonSerializer.Serialize(pos.EntryFills,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var trade = BacktestTrade.Create(
                backtestRunId: runId, isin: pending.Isin,
                entryTime: pos.EntryTime,
                lastEntryTime: pos.EntryFills[^1].Time,
                totalEntryQuantity: sellQty,
                weightedAvgEntryPrice: pos.WeightedAvgEntryPrice,
                totalBuyCommission: propBuyComm,
                entryFillsJson: entryFillsJson,
                exitTime: fillCandle.OpenTime,
                exitPrice: fillPrice,
                exitQuantity: sellQty,
                totalSellCommission: commission,
                exitReason: BacktestExitReason.Signal);

            state.Trades.Add(trade);

            if (sellQty >= pos.Quantity)
                state.Positions.Remove(pending.Isin);
            else
            {
                pos.Quantity -= sellQty;
                pos.TotalBuyCommission -= propBuyComm;
            }

            eventLogger.Log(runId, BacktestEventType.PositionClosed,
                BacktestEventSeverity.Info,
                $"موقعیت بسته شد: {sellQty:N0} سهم {pending.Isin} @ {fillPrice:N0}",
                isin: pending.Isin, candleTime: fillCandle.OpenTime, tradeId: trade.Id);
        }
    }

    /* ═══════════════════ ClosePosition ═══════════════════ */

    private void ClosePosition(
        BacktestState state, string isin, decimal triggerPrice,
        BacktestExitReason reason, DateTime time, Instrument instrument,
        BacktestSimulationRequest request, Guid runId, IBacktestEventLogger eventLogger)
    {
        if (!state.Positions.TryGetValue(isin, out var pos)) return;

        var fillPrice = _fillSimulator.FillPrice(
            OrderSide.Sell, triggerPrice, request.SlippagePercent);
        state.TotalSlippage += Math.Abs(fillPrice - triggerPrice) * pos.Quantity;

        var commissionRate = instrument.SellCommissionRate;
        var commission = FillMath.CalculateCommission(fillPrice, pos.Quantity, commissionRate);
        var proceeds = fillPrice * pos.Quantity - commission;
        state.Cash += proceeds;
        state.TotalCommission += commission;

        var entryFillsJson = JsonSerializer.Serialize(pos.EntryFills,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var trade = BacktestTrade.Create(
            backtestRunId: runId, isin: isin,
            entryTime: pos.EntryTime,
            lastEntryTime: pos.EntryFills[^1].Time,
            totalEntryQuantity: pos.Quantity,
            weightedAvgEntryPrice: pos.WeightedAvgEntryPrice,
            totalBuyCommission: pos.TotalBuyCommission,
            entryFillsJson: entryFillsJson,
            exitTime: time, exitPrice: fillPrice,
            exitQuantity: pos.Quantity,
            totalSellCommission: commission,
            exitReason: reason);

        state.Trades.Add(trade);
        state.Positions.Remove(isin);

        if (reason == BacktestExitReason.StopLoss || reason == BacktestExitReason.TakeProfit)
        {
            eventLogger.Log(runId, BacktestEventType.SLTPTriggered,
                reason == BacktestExitReason.StopLoss
                    ? BacktestEventSeverity.Warning : BacktestEventSeverity.Info,
                $"{(reason == BacktestExitReason.StopLoss ? "حد ضرر" : "حد سود")} فعال شد: " +
                $"{pos.Quantity:N0} سهم {isin} @ {fillPrice:N0}",
                isin: isin, candleTime: time, tradeId: trade.Id);
        }
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static decimal? ResolveStopLoss(
        BacktestSimulationRequest request, decimal entryPrice, Price? signalSL)
    {
        if (request.StopLossPrice.HasValue) return request.StopLossPrice.Value;
        if (request.StopLossPercentage.HasValue)
            return StopLossConfig.FromPercentage(request.StopLossPercentage.Value)
                .ResolveForEntry(new Price(entryPrice)).Value;
        return signalSL?.Value;
    }

    private static decimal? ResolveTakeProfit(
        BacktestSimulationRequest request, decimal entryPrice, Price? signalTP)
    {
        if (request.TakeProfitPrice.HasValue) return request.TakeProfitPrice.Value;
        if (request.TakeProfitPercentage.HasValue)
            return TakeProfitConfig.FromPercentage(request.TakeProfitPercentage.Value)
                .ResolveForEntry(new Price(entryPrice)).Value;
        return signalTP?.Value;
    }

    private static decimal ComputeEquity(BacktestState state)
    {
        var posValue = 0m;
        foreach (var (isin, pos) in state.Positions)
            if (state.LastPrices.TryGetValue(isin, out var last))
                posValue += pos.Quantity * last;
        return state.Cash + posValue;
    }

    private static void RecordEquity(
        BacktestState state, Guid runId, DateTime day, decimal prevDayEquity)
    {
        var equity = ComputeEquity(state);
        var posValue = equity - state.Cash;
        var dailyReturn = prevDayEquity > 0m
            ? (equity - prevDayEquity) / prevDayEquity * 100m : 0m;

        state.DailyEquities.Add(BacktestDailyEquity.Create(
            backtestRunId: runId,
            date: DateTime.SpecifyKind(day, DateTimeKind.Utc),
            equity: equity, cash: state.Cash, positionValue: posValue,
            dailyReturnPct: Math.Round(dailyReturn, 4)));
    }

    private static decimal ComputeDailyReturn(decimal current, decimal prev)
        => prev > 0m ? (current - prev) / prev * 100m : 0m;
}