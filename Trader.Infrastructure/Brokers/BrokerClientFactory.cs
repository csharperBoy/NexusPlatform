using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions;
using Trader.Domain.Enums;
using Trader.Infrastructure.Brokers.EasyTrader;

namespace Trader.Infrastructure.Brokers
{
    public class BrokerClientFactory : IBrokerClientFactory
    {
        private readonly IServiceProvider _sp;

        public BrokerClientFactory(IServiceProvider sp) => _sp = sp;

        public IBrokerClient GetClient(BrokerType brokerType)
        {
            return brokerType switch
            {
                BrokerType.EasyTrader => _sp.GetRequiredService<EasyTraderBrokerClient>(),
                _ => throw new NotSupportedException(
                    $"Broker '{brokerType}' is not supported.")
            };
        }
    }
}



