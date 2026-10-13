using Microsoft.Extensions.DependencyInjection;
using Trader.Application.Abstractions.Brokers;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Brokers
{
    public class BrokerClientFactory : IBrokerClientFactory
    {
        private readonly IServiceProvider _sp;

        public BrokerClientFactory(IServiceProvider sp) => _sp = sp;

        public T Get<T>(BrokerType brokerType) where T : class, IBroker
            => _sp.GetRequiredKeyedService<T>(brokerType);
    }
}