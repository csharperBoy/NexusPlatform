using Trader.Application.Abstractions.Execution;

namespace Trader.Infrastructure.Execution;

public sealed class OrderDispatcherResolver : IOrderDispatcherResolver
{
    private readonly BrokerOrderDispatcher _live;
    private readonly PaperBrokerOrderDispatcher _paper;

    public OrderDispatcherResolver(
        BrokerOrderDispatcher live,
        PaperBrokerOrderDispatcher paper)
    {
        _live = live;
        _paper = paper;
    }

    public IBrokerOrderDispatcher Resolve(bool isPaper)
        => isPaper ? _paper : _live;
}