using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Brokers;
using Trader.Domain.Enums;
using Trader.ExternalApiClient;
using Trader.ExternalApiClient.EasyTrader;

namespace Trader.Infrastructure.Brokers.EasyTrader.Internal
{
    /// <summary>
    /// پایه‌ی همه‌ی capabilityهای EasyTrader.
    /// فقط چیزهایی که همه لازم دارن اینجا هست.
    /// </summary>
    internal abstract class EasyTraderCapabilityBase : IBroker
    {
        protected EasyTraderApiClient Api { get; }
        protected ILogger Logger { get; }

        protected EasyTraderCapabilityBase(EasyTraderApiClient api, ILogger logger)
        {
            Api = api;
            Logger = logger;
        }

        public BrokerType BrokerType => BrokerType.EasyTrader;

        protected static EasyTraderSession AsSession(ExternalSystemSession s)
            => s as EasyTraderSession
               ?? throw new ArgumentException("Session is not EasyTraderSession");
    }
}