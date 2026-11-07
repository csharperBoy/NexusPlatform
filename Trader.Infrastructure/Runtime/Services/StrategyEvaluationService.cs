using Microsoft.Extensions.Logging;
using System.Text.Json;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.Events;

namespace Trader.Infrastructure.Runtime.Services;

/// <summary>
/// منطق کامل ارزیابی یک Strategy برای یک (Run, Symbol) — استخراج‌شده از
/// StrategyEvaluationStage (M6). مسئولیت‌ها:
///
///   1. Load primary candles (با فیلتر closed candles)
///   2. Apply per-candle currency transform
///   3. Load extra timeframes (sub-candle requirements)
///   4. Evaluate strategy
///   5. Enrich features با currency metadata
///   6. Create StrategyEvaluation (اگر features موجود بود)
///   7. Create Signalها (اگر Action != Hold)
///   8. Log evaluation-level events (CandlesLoaded، CurrencyTransformed، ...)
///
/// NOT مسئول:
///   • Persist (UoW) — Stage
///   • Push to channel — Stage
///   • SignalGenerated event — Stage (پس از persist)
///
/// Design note — چرا StrategyEvaluation.SourceEvaluationId و
/// Signal.SourceEvaluationId از eval.Id استفاده می‌کنند؟
///   Signal.Create با sourceEvaluationId: evalEntity?.Id صدا زده می‌شود،
///   یعنی FK نرم (بدون FK واقعی). LinkSignal فقط for audit/join آینده.
/// </summary>
public sealed class StrategyEvaluationService : IStrategyEvaluationService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ICandleCache _candleCache;
    private readonly ICandleTransformer _candleTransformer;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<StrategyEvaluationService> _logger;

    public StrategyEvaluationService(
        IMarketSnapshotStore snapshotStore,
        ICandleCache candleCache,
        ICandleTransformer candleTransformer,
        ITradingEventLogger eventLogger,
        ILogger<StrategyEvaluationService> logger)
    {
        _snapshotStore = snapshotStore;
        _candleCache = candleCache;
        _candleTransformer = candleTransformer;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task<StrategyEvaluationBatchResult> EvaluateAsync(
        RunRuntimeContext ctx,
        RunSymbolContext symbol,
        DateTime now,
        CancellationToken ct = default)
    {
        // ═══════════════════════════════════════════════════════════════
        // 1. SNAPSHOT
        // ═══════════════════════════════════════════════════════════════
        var snapshot = _snapshotStore.GetLatest(symbol.Isin);
        if (snapshot is null)
        {
            _logger.LogDebug("No snapshot for {Isin}; skipping.", symbol.Isin);
            return StrategyEvaluationBatchResult.Empty;
        }

        // ═══════════════════════════════════════════════════════════════
        // 2. PRIMARY CANDLES
        // ═══════════════════════════════════════════════════════════════
        IReadOnlyList<Candle> candles = Array.Empty<Candle>();
        IReadOnlyList<Candle> allCandles = Array.Empty<Candle>();

        if (ctx.Strategy.RequiredCandleCount > 0)
        {
            allCandles = await _candleCache.GetOrLoadAsync(
                symbol.Isin,
                ctx.PrimaryTimeFrame,
                ctx.Strategy.RequiredCandleCount,
                ct);

            if (ctx.Strategy.RequiresClosedCandlesOnly && allCandles.Count > 0)
            {
                var tfDuration = ctx.PrimaryTimeFrame.ToDuration();
                var nowUtc = DateTime.UtcNow;

                candles = allCandles
                    .Where(c => c.OpenTime + tfDuration <= nowUtc)
                    .ToList();

                if (candles.Count < allCandles.Count)
                {
                    _logger.LogTrace(
                        "Filtered {Diff} in-progress candle(s) for {Isin} tf={Tf} (kept {Kept}/{Total}).",
                        allCandles.Count - candles.Count,
                        symbol.Isin,
                        ctx.PrimaryTimeFrame,
                        candles.Count,
                        allCandles.Count);
                }
            }
            else
            {
                candles = allCandles;
            }

            if (candles.Count == 0)
            {
                _eventLogger.Log(
                    stage: TradingStage.StrategyEvaluation,
                    type: TradingEventType.CandlesLoadFailed,
                    severity: TradingEventSeverity.Warning,
                    message: $"داده‌ی کافی کندل برای «{symbol.Isin}» وجود ندارد " +
                             $"(موردنیاز: {ctx.Strategy.RequiredCandleCount}, " +
                             $"دریافت‌شده: {allCandles.Count}).",
                    payload: new
                    {
                        timeFrame = ctx.PrimaryTimeFrame.ToCode(),
                        requiredCount = ctx.Strategy.RequiredCandleCount,
                        rawCount = allCandles.Count,
                        closedCount = candles.Count,
                        isSubCandle = false,
                    },
                    runId: ctx.RunId,
                    isin: symbol.Isin);
            }
            else
            {
                _eventLogger.Log(
                    stage: TradingStage.StrategyEvaluation,
                    type: TradingEventType.CandlesLoaded,
                    severity: TradingEventSeverity.Debug,
                    message: $"{candles.Count} کندل {ctx.PrimaryTimeFrame.ToCode()} " +
                             $"«{symbol.Isin}» بارگذاری شد.",
                    payload: new
                    {
                        timeFrame = ctx.PrimaryTimeFrame.ToCode(),
                        candleCount = candles.Count,
                        lastCandleTime = candles[^1].OpenTime,
                        isSubCandle = false,
                    },
                    runId: ctx.RunId,
                    isin: symbol.Isin);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 3. CURRENCY TRANSFORM (Primary)
        // ═══════════════════════════════════════════════════════════════
        string? appliedCurrency = null;
        decimal? appliedRate = null;
        DateTime? appliedRateEffectiveAt = null;
        int? candleCountBeforeFirstRate = null;
        int? ratePointCount = null;

        var needsCurrencyTransform =
            !string.IsNullOrWhiteSpace(ctx.TargetCurrency)
            && ctx.TargetCurrency.Trim().ToUpperInvariant() != "IRR";

        if (needsCurrencyTransform && candles.Count > 0)
        {
            try
            {
                var transformation = await _candleTransformer.TransformAsync(
                    symbol.Isin,
                    candles,
                    ctx.TargetCurrency!,
                    now,
                    ct);

                appliedCurrency = ctx.TargetCurrency!.ToUpperInvariant();
                appliedRate = transformation.LastCandleRate;
                appliedRateEffectiveAt = transformation.LastCandleRateEffectiveAt;
                candleCountBeforeFirstRate = transformation.CandleCountBeforeFirstRate;
                ratePointCount = transformation.RatePointCount;

                candles = transformation.Candles;

                _eventLogger.Log(
                    stage: TradingStage.StrategyEvaluation,
                    type: TradingEventType.CurrencyTransformed,
                    severity: TradingEventSeverity.Debug,
                    message: $"قیمت‌های «{symbol.Isin}» از ریال به {appliedCurrency} تبدیل شدند " +
                             $"(Per-Candle، نرخ آخرین کندل: {FormatRateDisplay(appliedRate, appliedCurrency)}).",
                    payload: new
                    {
                        timeFrame = ctx.PrimaryTimeFrame.ToCode(),
                        targetCurrency = appliedCurrency,
                        lastCandleRate = appliedRate,
                        lastCandleRateDisplay = FormatRateDisplay(appliedRate, appliedCurrency),
                        lastCandleRateEffectiveAt = appliedRateEffectiveAt,
                        candleCount = candles.Count,
                        ratePointCount,
                        candleCountBeforeFirstRate,
                    },
                    runId: ctx.RunId,
                    isin: symbol.Isin);

                _logger.LogDebug(
                    "Applied per-candle currency transform: IRR→{Target} for {Isin} " +
                    "(lastCandleRate={Rate:E8}, ratePoints={RatePoints})",
                    appliedCurrency, symbol.Isin, appliedRate, ratePointCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Currency transform failed for {Isin} → {Target}. Skipping evaluation.",
                    symbol.Isin, ctx.TargetCurrency);

                _eventLogger.Log(
                    stage: TradingStage.StrategyEvaluation,
                    type: TradingEventType.StrategyEvaluationFailed,
                    severity: TradingEventSeverity.Error,
                    message: $"تبدیل ارز برای «{symbol.Isin}» به {ctx.TargetCurrency} " +
                             $"ناموفق بود: {ex.Message}",
                    payload: new
                    {
                        targetCurrency = ctx.TargetCurrency,
                        errorType = ex.GetType().Name,
                        errorMessage = ex.Message,
                    },
                    runId: ctx.RunId,
                    isin: symbol.Isin);

                return StrategyEvaluationBatchResult.Empty;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 4. EXTRA TIMEFRAMES (Sub-Candle)
        // ═══════════════════════════════════════════════════════════════
        IReadOnlyDictionary<TimeFrame, IReadOnlyList<Candle>>? extraTimeFrames = null;

        var requirements = ctx.Strategy.ExtraCandleRequirements;
        if (requirements.Count > 0)
        {
            var dict = new Dictionary<TimeFrame, IReadOnlyList<Candle>>();

            foreach (var req in requirements)
            {
                if (req.TimeFrame == ctx.PrimaryTimeFrame
                    || (int)req.TimeFrame >= (int)ctx.PrimaryTimeFrame)
                {
                    _logger.LogDebug(
                        "Skipping extra TF {TF} for {Isin}: >= primary {Primary}.",
                        req.TimeFrame, symbol.Isin, ctx.PrimaryTimeFrame);
                    continue;
                }

                try
                {
                    var allExtra = await _candleCache.GetOrLoadAsync(
                        symbol.Isin, req.TimeFrame, req.Count, ct);

                    IReadOnlyList<Candle> slice;
                    if (req.ClosedOnly && allExtra.Count > 0)
                    {
                        var tfDur = req.TimeFrame.ToDuration();
                        var nowUtc = DateTime.UtcNow;
                        slice = allExtra
                            .Where(c => c.OpenTime + tfDur <= nowUtc)
                            .ToList();
                    }
                    else
                    {
                        slice = allExtra;
                    }

                    if (needsCurrencyTransform && slice.Count > 0)
                    {
                        var trans = await _candleTransformer.TransformAsync(
                            symbol.Isin, slice, ctx.TargetCurrency!, now, ct);
                        slice = trans.Candles;
                    }

                    dict[req.TimeFrame] = slice;

                    if (slice.Count == 0)
                    {
                        _eventLogger.Log(
                            stage: TradingStage.StrategyEvaluation,
                            type: TradingEventType.CandlesLoadFailed,
                            severity: TradingEventSeverity.Warning,
                            message: $"داده‌ی Sub-Candle ({req.TimeFrame.ToCode()}) " +
                                     $"برای «{symbol.Isin}» کافی نیست " +
                                     $"(موردنیاز: {req.Count}, دریافت‌شده: {allExtra.Count}).",
                            payload: new
                            {
                                timeFrame = req.TimeFrame.ToCode(),
                                requiredCount = req.Count,
                                rawCount = allExtra.Count,
                                closedCount = slice.Count,
                                isSubCandle = true,
                            },
                            runId: ctx.RunId,
                            isin: symbol.Isin);
                    }
                    else
                    {
                        _eventLogger.Log(
                            stage: TradingStage.StrategyEvaluation,
                            type: TradingEventType.CandlesLoaded,
                            severity: TradingEventSeverity.Debug,
                            message: $"{slice.Count} کندل {req.TimeFrame.ToCode()} " +
                                     $"«{symbol.Isin}» (Sub-Candle) بارگذاری شد.",
                            payload: new
                            {
                                timeFrame = req.TimeFrame.ToCode(),
                                candleCount = slice.Count,
                                lastCandleTime = slice[^1].OpenTime,
                                requiredCount = req.Count,
                                isSubCandle = true,
                            },
                            runId: ctx.RunId,
                            isin: symbol.Isin);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to load extra TF {TF} for {Isin}. Skipping evaluation.",
                        req.TimeFrame, symbol.Isin);

                    _eventLogger.Log(
                        stage: TradingStage.StrategyEvaluation,
                        type: TradingEventType.CandlesLoadFailed,
                        severity: TradingEventSeverity.Warning,
                        message: $"دریافت کندل‌های Sub ({req.TimeFrame.ToCode()}) " +
                                 $"برای «{symbol.Isin}» ناموفق بود: {ex.Message}",
                        payload: new
                        {
                            timeFrame = req.TimeFrame.ToCode(),
                            requiredCount = req.Count,
                            isSubCandle = true,
                            errorType = ex.GetType().Name,
                            errorMessage = ex.Message,
                        },
                        runId: ctx.RunId,
                        isin: symbol.Isin);

                    return StrategyEvaluationBatchResult.Empty;
                }
            }

            extraTimeFrames = dict;
        }

        // ═══════════════════════════════════════════════════════════════
        // 5. STRATEGY EVALUATE
        // ═══════════════════════════════════════════════════════════════
        var input = new StrategyInput(
            Isin: symbol.Isin,
            LatestSnapshot: snapshot,
            Candles: candles,
            ExtraTimeFrames: extraTimeFrames,
            Configuration: ctx.StrategyConfig,
            TimeFrame: ctx.PrimaryTimeFrame,
            EvaluatedAt: now);

        var result = ctx.Strategy.Evaluate(input);

        // ═══════════════════════════════════════════════════════════════
        // 6. NoSignalGenerated event (Debug)
        // ═══════════════════════════════════════════════════════════════
        if (result.Features != null && result.Signals.Count == 0)
        {
            var rsiFeature = result.Features.TryGetValue("rsi", out var rsiVal)
                ? rsiVal?.ToString()
                : "—";

            _eventLogger.Log(
                stage: TradingStage.StrategyEvaluation,
                type: TradingEventType.NoSignalGenerated,
                severity: TradingEventSeverity.Debug,
                message: $"سیگنالی برای «{symbol.Isin}» صادر نشد. " +
                         $"RSI: {rsiFeature} " +
                         $"(قدرت کافی نبود یا در محدوده‌ی خنثی).",
                payload: new { features = result.Features },
                runId: ctx.RunId,
                isin: symbol.Isin);
        }

        // ═══════════════════════════════════════════════════════════════
        // 7. ENRICHMENT + EVAL ENTITY
        // ═══════════════════════════════════════════════════════════════
        StrategyEvaluation? evalEntity = null;
        if (result.Features != null)
        {
            if (appliedCurrency != null)
            {
                var enriched = new Dictionary<string, object?>(result.Features);
                enriched["targetCurrency"] = appliedCurrency;
                enriched["priceAtEvalIrr"] = snapshot.LastTradedPrice;
                if (appliedRate.HasValue)
                    enriched["appliedRate"] = appliedRate.Value;
                if (appliedRateEffectiveAt.HasValue)
                    enriched["appliedRateEffectiveAt"] = appliedRateEffectiveAt.Value;

                result = result with { Features = enriched };
            }

            var featuresJson = JsonSerializer.Serialize(result.Features, JsonOpts);

            var primaryAction = result.Signals.Count > 0
                ? result.Signals[0].Action
                : SignalAction.Hold;
            var primaryStrength = result.Signals.Count > 0
                ? result.Signals[0].Strength.Value
                : 0m;

            evalEntity = StrategyEvaluation.Create(
                runId: ctx.RunId,
                isin: symbol.Isin,
                strategyKey: ctx.StrategyKey,
                evaluatedAt: now,
                priceAtEval: snapshot.LastTradedPrice,
                featuresJson: featuresJson,
                action: primaryAction,
                strength: primaryStrength,
                marketSnapshotId: snapshot.Id);

            _logger.LogDebug(
                "Eval recorded: run={RunId} isin={Isin} action={Action} strength={Strength:0.##} featuresSize={Size}",
                ctx.RunId, symbol.Isin, evalEntity.Action,
                evalEntity.Strength, evalEntity.FeaturesJson.Length);
        }

        // ═══════════════════════════════════════════════════════════════
        // 8. SIGNALS
        // ═══════════════════════════════════════════════════════════════
        var signals = new List<Signal>();
        foreach (var s in result.Signals)
        {
            if (s.Action == SignalAction.Hold) continue;

            var signal = Signal.Create(
                runId: ctx.RunId,
                isin: s.Isin,
                strategyKey: ctx.StrategyKey,
                action: s.Action,
                strength: s.Strength,
                metadataJson: s.MetadataJson,
                sourceEvaluationId: evalEntity?.Id);

            signal.AddDomainEvent(new SignalGeneratedEvent(
                signal.Id,
                signal.RunId,
                signal.Isin,
                signal.Action,
                signal.Strength.Value,
                signal.ProducedAt));

            signals.Add(signal);
            evalEntity?.LinkSignal(signal.Id);
        }

        return new StrategyEvaluationBatchResult(evalEntity, signals);
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static string FormatRateDisplay(decimal? rate, string? targetCurrency)
    {
        if (rate is null || rate <= 0m) return "—";
        var inverted = 1m / rate.Value;
        return inverted >= 1m
            ? $"{inverted:N0} ریال = ۱ {targetCurrency}"
            : $"{rate.Value:E4}";
    }
}