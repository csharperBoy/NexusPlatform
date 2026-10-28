using Core.Application.Context;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.Events;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Runtime.Stages;

public sealed class StrategyEvaluationStage : IStrategyEvaluationStage
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly IRunExecutionContextCache _runCache;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ICandleCache _candleCache;
    private readonly ISignalChannel _signalChannel;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly UserDataContext _userContext;
    private readonly IUserContextCache _userContextCache;
    private readonly ICandleTransformer _candleTransformer;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<StrategyEvaluationStage> _logger;

    public StrategyEvaluationStage(
        IRunExecutionContextCache runCache,
        IMarketSnapshotStore snapshotStore,
        ICandleCache candleCache,
        ISignalChannel signalChannel,
        ITraderUnitOfWork<TraderDbContext> uow,
        UserDataContext userContext,
        IUserContextCache userContextCache,
        ICandleTransformer candleTransformer,
        ITradingEventLogger eventLogger,
        ILogger<StrategyEvaluationStage> logger)
    {
        _runCache = runCache;
        _snapshotStore = snapshotStore;
        _candleCache = candleCache;
        _signalChannel = signalChannel;
        _uow = uow;
        _userContext = userContext;
        _userContextCache = userContextCache;
        _candleTransformer = candleTransformer;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        var contexts = _runCache.GetAllActive();
        if (contexts.Count == 0) return;

        var now = DateTime.UtcNow;
        var pendingEnvelopes = new List<SignalEnvelope>();
        var evaluationCount = 0;
        var signalCount = 0;

        foreach (var ctx in contexts)
        {
            try
            {
                var userCtx = await _userContextCache.GetOrLoadAsync(
                    ctx.OwnerUserId, ct);
                _userContext.ApplyFrom(userCtx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to load user context for run {RunId}. Skipping.",
                    ctx.RunId);
                continue;
            }

            foreach (var symbol in ctx.Symbols)
            {
                try
                {
                    var (envs, evalSaved) = await EvaluateOnlyAsync(
                        ctx, symbol, now, ct);

                    evaluationCount += evalSaved ? 1 : 0;
                    signalCount += envs.Count;
                    pendingEnvelopes.AddRange(envs);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Evaluation failed: run={RunId} isin={Isin}",
                        ctx.RunId, symbol.Isin);
                }
            }
        }

        if (evaluationCount == 0 && pendingEnvelopes.Count == 0)
            return;

        // ─── یک Save برای همه (Evaluations + Signals) ───
        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist {EvalCount} evaluations and {SigCount} signals.",
                evaluationCount, pendingEnvelopes.Count);
            return;
        }

        // ─── Push Signals به Channel ───
        foreach (var envelope in pendingEnvelopes)
        {
            try
            {
                await _signalChannel.WriteAsync(envelope, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to push signal {SignalId} to channel.",
                    envelope.Signal.Id);
            }
        }

        _logger.LogInformation(
            "Stage 2: {EvalCount} evaluations, {SignalCount} signals across {RunCount} runs.",
            evaluationCount, signalCount, contexts.Count);
    }

    /* ═══════════════════ Core ═══════════════════ */

    private async Task<(List<SignalEnvelope> Envelopes, bool EvaluationSaved)>
        EvaluateOnlyAsync(
            RunRuntimeContext ctx,
            RunSymbolContext symbol,
            DateTime now,
            CancellationToken ct)
    {
        var envelopes = new List<SignalEnvelope>();

        var snapshot = _snapshotStore.GetLatest(symbol.Isin);
        if (snapshot is null)
        {
            _logger.LogDebug("No snapshot for {Isin}; skipping.", symbol.Isin);
            return (envelopes, false);
        }

        // ─── Candles اگر Strategy نیاز دارد ───
        IReadOnlyList<Candle> candles = Array.Empty<Candle>();
        IReadOnlyList<Candle> allCandles = Array.Empty<Candle>();

        if (ctx.Strategy.RequiredCandleCount > 0)
        {
            allCandles = await _candleCache.GetOrLoadAsync(
                symbol.Isin,
                ctx.PrimaryTimeFrame,
                ctx.Strategy.RequiredCandleCount,
                ct);

            // فیلتر closed
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

            // ─── CandlesLoaded / CandlesLoadFailed Event ───
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
                    },
                    runId: ctx.RunId,
                    isin: symbol.Isin);
            }
        }
        // ─── Currency Transformation (اختیاری) ───
        string? appliedCurrency = null;
        decimal? appliedRate = null;
        DateTime? appliedRateEffectiveAt = null;
        int? candleCountBeforeFirstRate = null;
        int? ratePointCount = null;

        if (!string.IsNullOrWhiteSpace(ctx.TargetCurrency)
            && ctx.TargetCurrency.Trim().ToUpperInvariant() != "IRR"
            && candles.Count > 0)
        {
            try
            {
                var transformation = await _candleTransformer.TransformAsync(
                    symbol.Isin,
                    candles,
                    ctx.TargetCurrency,
                    now,
                    ct);

                appliedCurrency = ctx.TargetCurrency.ToUpperInvariant();
                appliedRate = transformation.LastCandleRate;
                appliedRateEffectiveAt = transformation.LastCandleRateEffectiveAt;
                candleCountBeforeFirstRate = transformation.CandleCountBeforeFirstRate;
                ratePointCount = transformation.RatePointCount;

                candles = transformation.Candles;

                // ─── CurrencyTransformed Event ───
                _eventLogger.Log(
                    stage: TradingStage.StrategyEvaluation,
                    type: TradingEventType.CurrencyTransformed,
                    severity: TradingEventSeverity.Debug,
                    message: $"قیمت‌های «{symbol.Isin}» از ریال به {appliedCurrency} تبدیل شدند " +
                             $"(Per-Candle، نرخ آخرین کندل: {FormatRateDisplay(appliedRate, appliedCurrency)}).",
                    payload: new
                    {
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

                return (new List<SignalEnvelope>(), false);
            }
        }

        var input = new StrategyInput(
            Isin: symbol.Isin,
            LatestSnapshot: snapshot,
            Candles: candles,
            Configuration: ctx.StrategyConfig,
            TimeFrame: ctx.PrimaryTimeFrame,
            EvaluatedAt: now);

        var result = ctx.Strategy.Evaluate(input);

        // ─── NoSignalGenerated (Debug) — فقط اگر Features دارد و Hold است ───
        if (result.Features != null
            && result.Signals.Count == 0)
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
                payload: new
                {
                    features = result.Features,
                },
                runId: ctx.RunId,
                isin: symbol.Isin);
        }

        // ─── StrategyEvaluation (فقط اگر Features موجود باشد) ───
        StrategyEvaluation? evalEntity = null;
        if (result.Features != null)
        {
            // ─── ۱. Enrichment اول (قبل از Serialization) ───
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

            // ─── ۲. Serialization بعد از Enrichment ───
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

            await _uow.StrategyEvaluations.AddAsync(evalEntity);
        }

        // ─── Signals ───
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

            await _uow.Signals.AddAsync(signal);
            envelopes.Add(new SignalEnvelope(ctx.RunId, signal, now));
            // ─── SignalGenerated Event ───
            _eventLogger.Log(
                stage: TradingStage.StrategyEvaluation,
                type: TradingEventType.SignalGenerated,
                severity: TradingEventSeverity.Info,
                message: $"سیگنال {ActionToPersian(s.Action)} برای «{symbol.Isin}» در پلن «{ctx.Name}» " +
                         $"صادر شد. قدرت: {s.Strength.Value:0.##}",
                payload: new
                {
                    action = s.Action.ToString(),
                    strength = s.Strength.Value,
                    metadata = s.MetadataJson,
                },
                runId: ctx.RunId,
                isin: symbol.Isin,
                signalId: signal.Id);
            // Link دوطرفه
            evalEntity?.LinkSignal(signal.Id);
        }

        if (evalEntity != null)
        {
            _logger.LogDebug(
                "Eval recorded: run={RunId} isin={Isin} action={Action} strength={Strength:0.##} features={Features}",
                ctx.RunId, symbol.Isin, evalEntity.Action,
                evalEntity.Strength, evalEntity.FeaturesJson.Length);
        }

        return (envelopes, evalEntity != null);
    }
  
    private static string ActionToPersian(SignalAction action) => action switch
    {
        SignalAction.Buy => "خرید",
        SignalAction.Sell => "فروش",
        SignalAction.Hold => "خنثی",
        _ => "نامشخص"
    };
    private static string FormatRate(decimal? rate, string? targetCurrency)
    {
        if (rate is null || rate <= 0m) return "—";

        // نرخ استفاده‌شده = IRR → Target (مثل 3.759e-7)
        // نمایش بهتر: معکوسش = "چند ریال برای ۱ واحد ارز"
        var inverted = 1m / rate.Value;

        return inverted >= 1m
            ? $"{inverted:N0} ریال = ۱ {targetCurrency}"
            : $"{rate.Value:E4}";
    }
    /// <summary>
    /// نمایش خوانا از نرخ.
    /// appliedRate = IRR→Target (مثلاً 3.76e-7)
    /// معکوسش = "چند ریال برای ۱ واحد ارز" (مثلاً 2,660,000)
    /// </summary>
    private static string FormatRateDisplay(decimal? rate, string? targetCurrency)
    {
        if (rate is null || rate <= 0m) return "—";

        var inverted = 1m / rate.Value;

        return inverted >= 1m
            ? $"{inverted:N0} ریال = ۱ {targetCurrency}"
            : $"{rate.Value:E4}";
    }
}