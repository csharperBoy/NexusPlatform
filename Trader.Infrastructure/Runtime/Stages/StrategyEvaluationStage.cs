using Core.Application.Context;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Trader.Application.Abstractions;
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
    private readonly ILogger<StrategyEvaluationStage> _logger;

    public StrategyEvaluationStage(
        IRunExecutionContextCache runCache,
        IMarketSnapshotStore snapshotStore,
        ICandleCache candleCache,
        ISignalChannel signalChannel,
        ITraderUnitOfWork<TraderDbContext> uow,
        UserDataContext userContext,
        IUserContextCache userContextCache,
        ILogger<StrategyEvaluationStage> logger)
    {
        _runCache = runCache;
        _snapshotStore = snapshotStore;
        _candleCache = candleCache;
        _signalChannel = signalChannel;
        _uow = uow;
        _userContext = userContext;
        _userContextCache = userContextCache;
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
        if (ctx.Strategy.RequiredCandleCount > 0)
        {
            var allCandles = await _candleCache.GetOrLoadAsync(
                symbol.Isin,
                ctx.PrimaryTimeFrame,
                ctx.Strategy.RequiredCandleCount,
                ct);

            // ─── اگر Strategy فقط closed می‌خواهد، in-progress را کنار بگذار ───
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
                        "Filtered {Diff} in-progress candle(s) for {Isin} tf={Tf} " +
                        "(kept {Kept}/{Total}).",
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
        }

        var input = new StrategyInput(
            Isin: symbol.Isin,
            LatestSnapshot: snapshot,
            Candles: candles,
            Configuration: ctx.StrategyConfig,
            TimeFrame: ctx.PrimaryTimeFrame,
            EvaluatedAt: now);

        var result = ctx.Strategy.Evaluate(input);

        // ─── StrategyEvaluation (فقط اگر Features موجود باشد) ───
        StrategyEvaluation? evalEntity = null;
        if (result.Features != null)
        {
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
                marketSnapshotId: snapshot.Id);   // ← جدید

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
}