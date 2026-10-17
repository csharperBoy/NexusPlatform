using Trader.Domain.Entities;

namespace Trader.Application.Abstractions.Positions;

public interface IPositionManager
{
    /// <summary>اعمال یک Fill (از یک Trade مشخص) روی Position.</summary>
    Task ApplyTradeAsync(
        BrokerOrder order,
        OrderTrade trade,
        CancellationToken ct = default);

    /// <summary>اعمال کنسلی یک Trade که قبلاً اعمال شده بود.</summary>
    Task ApplyTradeCancelAsync(
        BrokerOrder order,
        OrderTrade trade,
        CancellationToken ct = default);
}