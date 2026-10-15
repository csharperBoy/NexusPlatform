using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.Runtime;

/// <summary>
/// Channel بین Stage 2 (Producer) و Stage 3 (Consumer).
/// Singleton، Bounded، Thread-safe.
/// </summary>
public interface ISignalChannel
{
    ValueTask WriteAsync(SignalEnvelope envelope, CancellationToken ct = default);
    IAsyncEnumerable<SignalEnvelope> ReadAllAsync(CancellationToken ct = default);
    int Count { get; }
}