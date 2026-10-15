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
    private readonly ILogger<StrategyEvaluationStage> _logger;

    public StrategyEvaluationStage(
        IRunExecutionContextCache runCache,
        IMarketSnapshotStore snapshotStore,
        ICandleCache candleCache,
        ISignalChannel signalChannel,
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<StrategyEvaluationStage> logger)
    {
        _runCache = runCache;
        _snapshotStore = snapshotStore;
        _candleCache = candleCache;
        _signalChannel = signalChannel;
        _uow = uow;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        var contexts = _runCache.GetAllActive();
        if (contexts.Count == 0) return;

        var now = DateTime.UtcNow;
        var totalSignals = 0;

        foreach (var ctx in contexts)
        {
            foreach (var symbol in ctx.Symbols)
            {
                try
                {
                    var produced = await EvaluateAndEmitAsync(ctx, symbol, now, ct);
                    totalSignals += produced;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // خطای یک Symbol بقیه را متوقف نمی‌کند
                    _logger.LogError(ex,
                        "Evaluation failed: run={RunId} isin={Isin}",
                        ctx.RunId, symbol.Isin);
                }
            }
        }

        if (totalSignals > 0)
        {
            _logger.LogInformation(
                "Stage 2: {SignalCount} signals produced across {RunCount} runs.",
                totalSignals, contexts.Count);
        }
    }

    /* ═══════════════════ Core ═══════════════════ */

    private async Task<int> EvaluateAndEmitAsync(
        RunRuntimeContext ctx,
        RunSymbolContext symbol,
        DateTime now,
        CancellationToken ct)
    {
        var snapshot = _snapshotStore.GetLatest(symbol.Isin);
        if (snapshot is null)
        {
            _logger.LogDebug(
                "No snapshot for {Isin}; skipping evaluation.", symbol.Isin);
            return 0;
        }

        // ─── Candles (اگر Strategy نیاز داشته باشد) ───
        IReadOnlyList<Candle> candles = Array.Empty<Candle>();
        if (ctx.Strategy.RequiredCandleCount > 0)
        {
            candles = await _candleCache.GetOrLoadAsync(
                symbol.Isin,
                ctx.PrimaryTimeFrame,
                ctx.Strategy.RequiredCandleCount,
                ct);
        }

        // ─── Evaluate ───
        var input = new StrategyInput(
            Isin: symbol.Isin,
            LatestSnapshot: snapshot,
            Candles: candles,
            Configuration: ctx.StrategyConfig,
            TimeFrame: ctx.PrimaryTimeFrame,
            EvaluatedAt: now);

        var result = ctx.Strategy.Evaluate(input);
        if (result.Signals.Count == 0) return 0;

        // ─── Persist Signals ───
        var pendingEnvelopes = new List<SignalEnvelope>(result.Signals.Count);

        foreach (var strategySignal in result.Signals)
        {
            if (strategySignal.Action == SignalAction.Hold) continue;

            var signal = Signal.Create(
                runId: ctx.RunId,
                isin: strategySignal.Isin,
                strategyKey: ctx.StrategyKey,
                action: strategySignal.Action,
                strength: strategySignal.Strength,
                metadataJson: strategySignal.MetadataJson);

            signal.AddDomainEvent(new SignalGeneratedEvent(
                signal.Id,
                signal.RunId,
                signal.Isin,
                signal.Action,
                signal.Strength.Value,
                signal.ProducedAt));

            await _uow.Signals.AddAsync(signal);
            pendingEnvelopes.Add(new SignalEnvelope(ctx.RunId, signal, now));
        }

        if (pendingEnvelopes.Count == 0) return 0;

        // ─── Save اول، Push بعد ───
        // اگر Process بین Save و Push بمیرد، Signal در DB هست ولی Stage 3 آن را ندیده.
        // این حالت با Recovery در Startup قابل جبران است (Signalهای Consumed‌نشده).
        await _uow.SaveChangesAsync(ct);

        foreach (var envelope in pendingEnvelopes)
        {
            await _signalChannel.WriteAsync(envelope, ct);
        }

        _logger.LogInformation(
            "Signal(s) emitted: run={RunId} isin={Isin} count={Count} actions=[{Actions}]",
            ctx.RunId, symbol.Isin, pendingEnvelopes.Count,
            string.Join(",", pendingEnvelopes.Select(e => e.Signal.Action)));

        return pendingEnvelopes.Count;
    }
}