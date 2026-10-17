namespace Trader.Application.Abstractions.Runtime;

/// <summary>
/// Stage 3: مصرف Signalها از Channel و تبدیل به OrderIntent و BrokerOrder.
/// </summary>
public interface IExecutionStage
{
    Task RunOnceAsync(CancellationToken ct = default);
}