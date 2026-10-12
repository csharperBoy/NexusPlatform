using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Brokers
{
    public interface IBrokerClientFactory
    {
        IBrokerClient GetClient(BrokerType brokerType);
        T Get<T>(BrokerType brokerType) where T : class, IBroker;
    }
}