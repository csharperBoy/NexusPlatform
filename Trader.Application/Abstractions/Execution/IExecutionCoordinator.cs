using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// مسئول پردازش Signalها و ارسال سفارش. یک Singleton که Channel را مصرف می‌کند.
/// </summary>
public interface IExecutionCoordinator
{
    /// <summary>پردازش یک Signal. Idempotent به ازای RunId+SignalId.</summary>
    Task HandleAsync(SignalEnvelope envelope, CancellationToken ct = default);

    /// <summary>بستن Positionهای یک Run هنگام Stop با ClosePositions.</summary>
    Task CloseRunPositionsAsync(Guid runId, CancellationToken ct = default);
}