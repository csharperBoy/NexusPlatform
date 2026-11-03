using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.Positions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.ExternalApiClient;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Execution;

/// <summary>
/// Dispatcher شبیه‌سازی برای Paper Trading.
/// سفارش بلافاصله Fill می‌شود در intent.Price (BestAsk/Bid از Snapshot).
/// Position مستقیماً باز می‌شود. Stage 4 آن را Skip می‌کند (Terminal).
/// </summary>
public sealed class PaperBrokerOrderDispatcher : IBrokerOrderDispatcher
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IPositionManager _positionManager;
    private readonly ILogger<PaperBrokerOrderDispatcher> _logger;

    public PaperBrokerOrderDispatcher(
        ITraderUnitOfWork<TraderDbContext> uow,
        IPositionManager positionManager,
        ILogger<PaperBrokerOrderDispatcher> logger)
    {
        _uow = uow;
        _positionManager = positionManager;
        _logger = logger;
    }

    public async Task<BrokerOrder> DispatchAsync(
        OrderIntent intent,
        BrokerType brokerType,
        ExternalSystemSession session,
        CancellationToken ct = default)
    {
        // ─── ۱. BrokerOrder (Paper) ───
        var brokerOrder = BrokerOrder.CreatePending(
            runId: intent.RunId,
            traderAccountId: intent.TraderAccountId,
            orderIntentId: intent.Id,
            isin: intent.Isin,
            side: intent.Side,
            price: intent.Price,
            quantity: intent.Quantity,
            isPaper: true);

        await _uow.BrokerOrders.AddAsync(brokerOrder);

        // ─── ۲. Simulate Immediate Fill ───
        var fillPrice = intent.Price.Value;
        var fillQty = intent.Quantity.Value;

        brokerOrder.AssignBrokerOrderId($"PAPER-{Guid.NewGuid():N}");
        brokerOrder.ApplyStateUpdate(
            OrderStateKind.FullyExecuted,
            new Quantity(fillQty));

        intent.MarkDispatched();
        intent.MarkAcknowledged();

        // ─── ۳. Create OrderTrade ───
        var tradeNumber = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000
                          + Random.Shared.Next(0, 1000);

        var trade = OrderTrade.Create(
            brokerOrderId: brokerOrder.Id,
            tradeNumber: tradeNumber,
            isin: intent.Isin,
            side: intent.Side,
            price: intent.Price,
            quantity: new Quantity(fillQty),
            tradeDate: DateTimeOffset.UtcNow);

        await _uow.OrderTrades.AddAsync(trade);

        // ─── ۴. Apply to Position ───
        await _positionManager.ApplyTradeAsync(brokerOrder, trade, ct);

        _logger.LogInformation(
            "PAPER fill: run={RunId} isin={Isin} side={Side} qty={Qty} price={Price}",
            intent.RunId, intent.Isin, intent.Side, fillQty, fillPrice);

        return brokerOrder;
    }
}