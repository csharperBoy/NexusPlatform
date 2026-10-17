using Trader.Domain.Entities;
using Trader.ExternalApiClient;

namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// نگاشت OrderIntent به درخواست Broker و ارسال آن.
/// </summary>
public interface IBrokerOrderDispatcher
{
    Task<BrokerOrder> DispatchAsync(
        OrderIntent intent,
        Trader.Domain.Enums.BrokerType brokerType,
        ExternalSystemSession session,
        CancellationToken ct = default);
}