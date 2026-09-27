using Trader.Domain.Enums;

namespace Trader.Application.Abstractions
{
    public interface IBrokerClientFactory
    {
        IBrokerClient GetClient(BrokerType brokerType);
    }
}