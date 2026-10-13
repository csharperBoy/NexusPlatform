using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Brokers
{
    /// <summary>
    /// Resolve یک capability بروکر بر اساس نوع بروکر در زمان اجرا.
    /// </summary>
    public interface IBrokerClientFactory
    {
        T Get<T>(BrokerType brokerType) where T : class, IBroker;
    }
}