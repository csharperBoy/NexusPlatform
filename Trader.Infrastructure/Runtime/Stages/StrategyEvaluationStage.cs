using Core.Application.Context;
using Microsoft.Extensions.Logging;
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
    private readonly IRunExecutionContextCache _runCache;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ICandleCache _candleCache;
    private readonly ISignalChannel _signalChannel;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly UserDataContext _userContext;
    private readonly ILogger<StrategyEvaluationStage> _logger;
    private readonly IUserContextCache _userContextCache;

    public StrategyEvaluationStage(
        IRunExecutionContextCache runCache,
        IMarketSnapshotStore snapshotStore,
        ICandleCache candleCache,
        ISignalChannel signalChannel,
        ITraderUnitOfWork<TraderDbContext> uow,
        UserDataContext userContext,
        ILogger<StrategyEvaluationStage> logger)
    {
        _runCache = runCache;
        _snapshotStore = snapshotStore;
        _candleCache = candleCache;
        _signalChannel = signalChannel;
        _uow = uow;
        _userContext = userContext;
        _logger = logger;
    }
    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        var contexts = _runCache.GetAllActive();
        if (contexts.Count == 0) return;

        var now = DateTime.UtcNow;
        var pendingEnvelopes = new List<SignalEnvelope>();

        foreach (var ctx in contexts)
        {
            // ─── لود کامل User Context از Cache ───
            try
            {
                var userCtx = await _userContextCache.GetOrLoadAsync(
             ctx.OwnerUserId, ct);
                _userContext.ApplyFrom(userCtx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to load user context for run {RunId} owner={OwnerId}. Skipping run.",
                    ctx.RunId, ctx.OwnerUserId);
                continue;
            }
            foreach (var symbol in ctx.Symbols)
            {
                try
                {
                    var envs = await EvaluateOnlyAsync(ctx, symbol, now, ct);
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

        if (pendingEnvelopes.Count == 0) return;

        try
        {
            await _uow.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to persist {Count} signals.", pendingEnvelopes.Count);
            return;
        }

        foreach (var envelope in pendingEnvelopes)
        {
            try
            {
                await _signalChannel.WriteAsync(envelope, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to push signal {SignalId} to channel. " +
                    "Signal remains in DB; Recovery will pick it up.",
                    envelope.Signal.Id);
            }
        }

        _logger.LogInformation(
            "Stage 2: {SignalCount} signals emitted across {RunCount} runs.",
            pendingEnvelopes.Count, contexts.Count);
    }
    /* ═══════════════════ Core (بدون Save) ═══════════════════ */

    private async Task<List<SignalEnvelope>> EvaluateOnlyAsync(
        RunRuntimeContext ctx,
        RunSymbolContext symbol,
        DateTime now,
        CancellationToken ct)
    {
        var result = new List<SignalEnvelope>();

        var snapshot = _snapshotStore.GetLatest(symbol.Isin);
        if (snapshot is null)
        {
            _logger.LogDebug(
                "No snapshot for {Isin}; skipping.", symbol.Isin);
            return result;
        }

        // Candles اگر Strategy نیاز دارد
        IReadOnlyList<Candle> candles = Array.Empty<Candle>();
        if (ctx.Strategy.RequiredCandleCount > 0)
        {
            candles = await _candleCache.GetOrLoadAsync(
                symbol.Isin,
                ctx.PrimaryTimeFrame,
                ctx.Strategy.RequiredCandleCount,
                ct);
        }

        // Evaluate
        var input = new StrategyInput(
            Isin: symbol.Isin,
            LatestSnapshot: snapshot,
            Candles: candles,
            Configuration: ctx.StrategyConfig,
            TimeFrame: ctx.PrimaryTimeFrame,
            EvaluatedAt: now);

        var evaluation = ctx.Strategy.Evaluate(input);
        if (evaluation.Signals.Count == 0) return result;

        // Persist در Change Tracker (بدون Save)
        foreach (var s in evaluation.Signals)
        {
            if (s.Action == SignalAction.Hold) continue;

            var signal = Signal.Create(
                runId: ctx.RunId,
                isin: s.Isin,
                strategyKey: ctx.StrategyKey,
                action: s.Action,
                strength: s.Strength,
                metadataJson: s.MetadataJson);

            signal.AddDomainEvent(new SignalGeneratedEvent(
                signal.Id,
                signal.RunId,
                signal.Isin,
                signal.Action,
                signal.Strength.Value,
                signal.ProducedAt));

            await _uow.Signals.AddAsync(signal);
            result.Add(new SignalEnvelope(ctx.RunId, signal, now));
        }

        return result;
    }
}