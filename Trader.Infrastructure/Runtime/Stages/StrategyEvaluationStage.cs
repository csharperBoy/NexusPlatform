using Core.Application.Context;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Runtime.Stages;

/// <summary>
/// Stage 2 — Orchestration فقط.
///
/// مسئولیت‌ها:
///   1. برای هر Run فعال، User Context را بارگذاری کن
///   2. برای هر Symbol در Run، IStrategyEvaluationService.EvaluateAsync صدا بزن
///   3. Evaluationها و Signalها را Persist کن
///   4. SignalGenerated Event را Log کن
///   5. Envelope بساز و به Channel Push کن
///
/// منطق ارزیابی (load candles, transform currency, evaluate, create entities)
/// در IStrategyEvaluationService است.
/// </summary>
public sealed class StrategyEvaluationStage : IStrategyEvaluationStage
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly IUserContextCache _userContextCache;
    private readonly UserDataContext _userContext;
    private readonly IStrategyEvaluationService _evaluationService;
    private readonly ISignalChannel _signalChannel;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<StrategyEvaluationStage> _logger;

    public StrategyEvaluationStage(
        IRunExecutionContextCache runCache,
        IUserContextCache userContextCache,
        UserDataContext userContext,
        IStrategyEvaluationService evaluationService,
        ISignalChannel signalChannel,
        ITraderUnitOfWork<TraderDbContext> uow,
        ITradingEventLogger eventLogger,
        ILogger<StrategyEvaluationStage> logger)
    {
        _runCache = runCache;
        _userContextCache = userContextCache;
        _userContext = userContext;
        _evaluationService = evaluationService;
        _signalChannel = signalChannel;
        _uow = uow;
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
                    var batch = await _evaluationService.EvaluateAsync(
                        ctx, symbol, now, ct);

                    // Persist Evaluation (FK مقدم بر Signal)
                    if (batch.Evaluation is not null)
                    {
                        await _uow.StrategyEvaluations.AddAsync(batch.Evaluation);
                        evaluationCount++;
                    }

                    // Persist Signals + build envelopes
                    foreach (var signal in batch.Signals)
                    {
                        await _uow.Signals.AddAsync(signal);
                        pendingEnvelopes.Add(new SignalEnvelope(
                            ctx.RunId, signal, now));

                        _eventLogger.Log(
                            stage: TradingStage.StrategyEvaluation,
                            type: TradingEventType.SignalGenerated,
                            severity: TradingEventSeverity.Info,
                            message: $"سیگنال {ActionToPersian(signal.Action)} برای " +
                                     $"«{signal.Isin}» در پلن «{ctx.Name}» صادر شد. " +
                                     $"قدرت: {signal.Strength.Value:0.##}",
                            payload: new
                            {
                                action = signal.Action.ToString(),
                                strength = signal.Strength.Value,
                                metadata = signal.MetadataJson,
                            },
                            runId: ctx.RunId,
                            isin: signal.Isin,
                            signalId: signal.Id);
                    }

                    signalCount += batch.Signals.Count;
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

        // ─── یک Save برای همه (Evaluations + Signals + Domain Events) ───
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

    private static string ActionToPersian(SignalAction action) => action switch
    {
        SignalAction.Buy => "خرید",
        SignalAction.Sell => "فروش",
        SignalAction.Hold => "خنثی",
        _ => "نامشخص"
    };
}