using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.Runtime;

namespace Trader.Infrastructure.Runtime.Stages;

/// <summary>
/// Stage 3: تمام Signalهای در Channel را در این Tick Drain و پردازش می‌کند.
/// </summary>
public sealed class ExecutionStage : IExecutionStage
{
    private const int MaxSignalsPerTick = 200;

    private readonly ISignalChannel _channel;
    private readonly IExecutionCoordinator _coordinator;
    private readonly ILogger<ExecutionStage> _logger;

    public ExecutionStage(
        ISignalChannel channel,
        IExecutionCoordinator coordinator,
        ILogger<ExecutionStage> logger)
    {
        _channel = channel;
        _coordinator = coordinator;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        if (_channel.Count == 0) return;

        var processed = 0;
        var enumerator = _channel.ReadAllAsync(ct).GetAsyncEnumerator(ct);

        try
        {
            while (processed < MaxSignalsPerTick)
            {
                // Non-blocking check: اگر Channel خالی شد، خارج شو
                if (_channel.Count == 0) break;

                if (!await enumerator.MoveNextAsync()) break;

                var envelope = enumerator.Current;
                try
                {
                    await _coordinator.HandleAsync(envelope, ct);
                    processed++;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Execution failed for signal {SignalId} run {RunId}.",
                        envelope.Signal.Id, envelope.RunId);
                }
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        if (processed > 0)
        {
            _logger.LogInformation(
                "Stage 3: {Count} signals executed.", processed);
        }
    }
}