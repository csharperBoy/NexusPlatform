using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Runtime;

public sealed class SignalChannel : ISignalChannel
{
    private readonly Channel<SignalEnvelope> _channel;

    public SignalChannel(IOptions<TradingRuntimeOptions> options)
    {
        var capacity = options.Value.SignalChannelCapacity;

        _channel = Channel.CreateBounded<SignalEnvelope>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });
    }

    public ValueTask WriteAsync(SignalEnvelope envelope, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(envelope, ct);

    public IAsyncEnumerable<SignalEnvelope> ReadAllAsync(CancellationToken ct = default)
        => _channel.Reader.ReadAllAsync(ct);

    public int Count => _channel.Reader.Count;
}