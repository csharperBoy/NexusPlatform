using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Application.Specifications.OrderIntents;
using Trader.Application.Specifications.Signals;
using Trader.Application.Specifications.TradingRuns;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Runtime.Recovery;

public sealed class RuntimeRecoveryService : IRuntimeRecoveryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ITradingRunManager _runManager;
    private readonly ISignalChannel _signalChannel;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<RuntimeRecoveryService> _logger;

    public RuntimeRecoveryService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ITradingRunManager runManager,
        ISignalChannel signalChannel,
        ITradingEventLogger eventLogger,
        ILogger<RuntimeRecoveryService> logger)
    {
        _uow = uow;
        _runManager = runManager;
        _signalChannel = signalChannel;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task RecoverAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Runtime recovery starting...");

        _eventLogger.Log(
            stage: TradingStage.Recovery,
            type: TradingEventType.RecoveryStarted,
            severity: TradingEventSeverity.Info,
            message: "فرآیند بازیابی Runtime شروع شد.");

        try
        {
            var runningRunIds = await LoadRunningRunsAsync(ct);
            if (runningRunIds.Count == 0)
            {
                _logger.LogInformation(
                    "Runtime recovery: no running runs to restore.");

                _eventLogger.Log(
                    stage: TradingStage.Recovery,
                    type: TradingEventType.RecoveryCompleted,
                    severity: TradingEventSeverity.Info,
                    message: "بازیابی کامل شد: هیچ پلن فعالی برای بازیابی نبود.",
                    payload: new { restoredRuns = 0, repushedSignals = 0 });

                await _eventLogger.FlushAsync(ct);
                return;
            }

            var repushed = await RecoverUnconsumedSignalsAsync(runningRunIds, ct);

            _logger.LogInformation(
                "Runtime recovery completed. Restored {RunCount} runs, re-pushed {SignalCount} signals.",
                runningRunIds.Count, repushed);

            _eventLogger.Log(
                stage: TradingStage.Recovery,
                type: TradingEventType.RecoveryCompleted,
                severity: TradingEventSeverity.Info,
                message: $"بازیابی کامل شد: {runningRunIds.Count} پلن بازیابی شد، " +
                         $"{repushed} سیگنال Re-Push شد.",
                payload: new
                {
                    restoredRuns = runningRunIds.Count,
                    repushedSignals = repushed
                });
        }
        catch (Exception ex)
        {
            _eventLogger.Log(
                stage: TradingStage.Recovery,
                type: TradingEventType.RecoveryFailed,
                severity: TradingEventSeverity.Error,
                message: $"بازیابی ناموفق بود: {ex.Message}",
                payload: new
                {
                    errorType = ex.GetType().Name,
                    errorMessage = ex.Message
                });
            throw;
        }
        finally
        {
            try { await _eventLogger.FlushAsync(ct); }
            catch (Exception flushEx)
            {
                _logger.LogError(flushEx,
                    "Failed to flush recovery events.");
            }
        }
    }

    private async Task<HashSet<Guid>> LoadRunningRunsAsync(CancellationToken ct)
    {
        var spec = new RunningTradingRunsSpec();
        var runs = (await _uow.TradingRunSpecs.ListBySpecAsync(spec)).ToList();

        if (runs.Count == 0) return new HashSet<Guid>();

        _logger.LogInformation(
            "Runtime recovery: {Count} runs in Running state.", runs.Count);

        var restored = new HashSet<Guid>();

        foreach (var run in runs)
        {
            try
            {
                await _runManager.StartAsync(run.Id, ct);
                restored.Add(run.Id);

                _logger.LogInformation(
                    "Restored run {RunId} ({Name})", run.Id, run.Name);

                _eventLogger.Log(
                    stage: TradingStage.Recovery,
                    type: TradingEventType.RunRestoredFromRecovery,
                    severity: TradingEventSeverity.Info,
                    message: $"پلن «{run.Name}» از بازیابی Startup فعال شد.",
                    runId: run.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to restore run {RunId} ({Name}). Marking faulted.",
                    run.Id, run.Name);

                _eventLogger.Log(
                    stage: TradingStage.Recovery,
                    type: TradingEventType.RecoveryFailed,
                    severity: TradingEventSeverity.Error,
                    message: $"بازیابی پلن «{run.Name}» ناموفق بود: {ex.Message}",
                    payload: new
                    {
                        errorType = ex.GetType().Name,
                        errorMessage = ex.Message
                    },
                    runId: run.Id);

                try
                {
                    run.MarkFaulted($"Recovery failed: {ex.Message}");
                    await _uow.TradingRuns.UpdateAsync(run);
                    await _uow.SaveChangesAsync(ct);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx,
                        "Failed to mark run {RunId} as faulted.", run.Id);
                }
            }
        }

        return restored;
    }

    private async Task<int> RecoverUnconsumedSignalsAsync(
    HashSet<Guid> runningRunIds, CancellationToken ct)
    {
        var spec = new UnconsumedSignalsByRunsSpec(runningRunIds);
        var signals = (await _uow.SignalSpecs.ListBySpecAsync(spec)).ToList();

        if (signals.Count == 0) return 0;

        _logger.LogInformation(
            "Runtime recovery: {Count} unconsumed signals found.", signals.Count);

        var repushed = 0;

        foreach (var signal in signals)
        {
            try
            {
                var intentSpec = new OrderIntentsBySignalSpec(signal.Id);
                var existingIntents = (await _uow.OrderIntentSpecs
                    .ListBySpecAsync(intentSpec)).ToList();

                if (existingIntents.Count > 0)
                {
                    signal.MarkConsumed();
                    await _uow.Signals.UpdateAsync(signal);
                    _logger.LogWarning(
                        "Signal {SignalId} has existing OrderIntents; marking consumed.",
                        signal.Id);
                }
                else
                {
                    var envelope = new SignalEnvelope(
                        signal.RunId, signal, signal.ProducedAt);
                    await _signalChannel.WriteAsync(envelope, ct);
                    repushed++;
                    _logger.LogInformation(
                        "Re-pushed signal {SignalId}.", signal.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to recover signal {SignalId}.", signal.Id);
            }
        }

        await _uow.SaveChangesAsync(ct);
        return repushed;
    }
}