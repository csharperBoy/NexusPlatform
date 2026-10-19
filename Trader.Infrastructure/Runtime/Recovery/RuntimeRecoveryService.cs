using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
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
    private readonly ILogger<RuntimeRecoveryService> _logger;

    public RuntimeRecoveryService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ITradingRunManager runManager,
        ISignalChannel signalChannel,
        ILogger<RuntimeRecoveryService> logger)
    {
        _uow = uow;
        _runManager = runManager;
        _signalChannel = signalChannel;
        _logger = logger;
    }

    public async Task RecoverAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Runtime recovery starting...");

        var runningRunIds = await LoadRunningRunsAsync(ct);
        if (runningRunIds.Count == 0)
        {
            _logger.LogInformation("Runtime recovery: no running runs to restore.");
            return;
        }

        await RecoverUnconsumedSignalsAsync(runningRunIds, ct);

        _logger.LogInformation(
            "Runtime recovery completed. Restored {RunCount} runs.", runningRunIds.Count);
    }

    /* ═══════════════════ Phase 1: Restore Run Contexts ═══════════════════ */

    private async Task<HashSet<Guid>> LoadRunningRunsAsync(CancellationToken ct)
    {
        var spec = new RunningTradingRunsSpec();
        var runs = await _uow.TradingRunSpecs.ListBySpecAsync(spec);

        if (runs.Count() == 0)
            return new HashSet<Guid>();

        _logger.LogInformation(
            "Runtime recovery: {Count} runs in Running state.", runs.Count());

        var restored = new HashSet<Guid>();

        foreach (var run in runs)
        {
            try
            {
                await _runManager.StartAsync(run.Id, ct);
                restored.Add(run.Id);

                _logger.LogInformation(
                    "Restored run {RunId} ({Name})", run.Id, run.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to restore run {RunId} ({Name}). Marking faulted.",
                    run.Id, run.Name);

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

    /* ═══════════════════ Phase 2: Recover Unconsumed Signals ═══════════════════ */

    private async Task RecoverUnconsumedSignalsAsync(
        HashSet<Guid> runningRunIds, CancellationToken ct)
    {
        // ─── Signalهای Consume‌نشده برای Runهای فعال ───
        var spec = new UnconsumedSignalsByRunsSpec(runningRunIds);
        var signals = await _uow.SignalSpecs.ListBySpecAsync(spec);

        if (signals.Count() == 0)
        {
            _logger.LogDebug("Runtime recovery: no unconsumed signals.");
            return;
        }

        _logger.LogInformation(
            "Runtime recovery: {Count} unconsumed signals found.", signals.Count());

        var repushedCount = 0;
        var skippedCount = 0;

        foreach (var signal in signals)
        {
            try
            {
                // ─── آیا OrderIntent برای این Signal ساخته شده؟ ───
                var intentSpec = new OrderIntentsBySignalSpec(signal.Id);
                var existingIntents = await _uow.OrderIntentSpecs.ListBySpecAsync(intentSpec);

                if (existingIntents.Count() > 0)
                {
                    // سناریو C/D: قبلاً OrderIntent ساخته شده — Skip
                    // پذیرش ریسک: اگر برای بعضی Accountها Intent نساخته باشد، آن‌ها Skip می‌شوند.
                    signal.MarkConsumed();
                    await _uow.Signals.UpdateAsync(signal);
                    skippedCount++;

                    _logger.LogWarning(
                        "Signal {SignalId} (run={RunId} isin={Isin}) has existing OrderIntents " +
                        "({Count}); marking as consumed to prevent duplicate execution.",
                        signal.Id, signal.RunId, signal.Isin, existingIntents.Count());
                }
                else
                {
                    // سناریو B: هیچ OrderIntent — Re-Push
                    var envelope = new SignalEnvelope(
                        signal.RunId, signal, signal.ProducedAt);

                    await _signalChannel.WriteAsync(envelope, ct);
                    repushedCount++;

                    _logger.LogInformation(
                        "Re-pushed signal {SignalId} (run={RunId} isin={Isin} action={Action}).",
                        signal.Id, signal.RunId, signal.Isin, signal.Action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to recover signal {SignalId}. Skipping.", signal.Id);
            }
        }

        if (skippedCount > 0)
        {
            try
            {
                await _uow.SaveChangesAsync(ct);
                _logger.LogInformation(
                    "Marked {Count} signals as consumed.", skippedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to persist consumed state for recovered signals.");
            }
        }

        _logger.LogInformation(
            "Signal recovery: {RePushed} re-pushed, {Skipped} skipped (already consumed).",
            repushedCount, skippedCount);
    }
}