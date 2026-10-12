using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions.Brokers;
using Trader.Domain.Enums;
using Trader.Infrastructure.Brokers.EasyTrader;

namespace Trader.Infrastructure.Brokers
{
    public class BrokerClientFactory : IBrokerClientFactory
    {
        private readonly IServiceProvider _sp;

        public BrokerClientFactory(IServiceProvider sp) => _sp = sp;

        // دریافت کل نمای بروکر (Aggregate)
        public IBrokerClient GetClient(BrokerType brokerType)
            => _sp.GetRequiredKeyedService<IBrokerClient>(brokerType);

        // دریافت یک capability خاص
        public T Get<T>(BrokerType brokerType) where T : class, IBroker
            => _sp.GetRequiredKeyedService<T>(brokerType);
    }
}



