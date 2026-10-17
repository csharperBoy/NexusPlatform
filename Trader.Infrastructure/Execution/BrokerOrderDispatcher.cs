using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Commands;
using Trader.Application.Abstractions.Execution;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.ExternalApiClient;

namespace Trader.Infrastructure.Execution;

public sealed class BrokerOrderDispatcher : IBrokerOrderDispatcher
{
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly ITraderUnitOfWork<Data.TraderDbContext> _uow;
    private readonly ILogger<BrokerOrderDispatcher> _logger;

    public BrokerOrderDispatcher(
        IBrokerClientFactory brokerFactory,
        ITraderUnitOfWork<Data.TraderDbContext> uow,
        ILogger<BrokerOrderDispatcher> logger)
    {
        _brokerFactory = brokerFactory;
        _uow = uow;
        _logger = logger;
    }

    public async Task<BrokerOrder> DispatchAsync(
        OrderIntent intent,
        BrokerType brokerType,
        ExternalSystemSession session,
        CancellationToken ct = default)
    {
        var brokerOrder = BrokerOrder.CreatePending(
            runId: intent.RunId,
            traderAccountId: intent.TraderAccountId,
            orderIntentId: intent.Id,
            isin: intent.Isin,
            side: intent.Side,
            price: intent.Price,
            quantity: intent.Quantity);

        await _uow.BrokerOrders.AddAsync(brokerOrder);

        var commands = _brokerFactory.Get<IBrokerTradingCommands>(brokerType);

        try
        {
            intent.MarkDispatched();

            var result = intent.Side == OrderSide.Buy
    ? await commands.SendBuyOrderAsync(
        session, intent.Isin, (long)intent.Price.Value, intent.Quantity.Value, ct)
    : await commands.SendSellOrderAsync(
        session, intent.Isin, (long)intent.Price.Value, intent.Quantity.Value, ct);

            if (!result.IsSuccessful || string.IsNullOrWhiteSpace(result.OrderId))
            {
                brokerOrder.ApplyStateUpdate(OrderStateKind.Rejected, Quantity.Zero);
                intent.MarkRejected(
                    result.Message ?? result.ErrorName ?? "Broker rejected the order.");

                _logger.LogWarning(
                    "Broker rejected order: run={RunId} isin={Isin} code={Code} msg={Message}",
                    intent.RunId, intent.Isin, result.ErrorCode, result.Message);
            }
            else
            {
                brokerOrder.AssignBrokerOrderId(result.OrderId);
                brokerOrder.ApplyStateUpdate(OrderStateKind.Pending, Quantity.Zero);
                intent.MarkAcknowledged();

                _logger.LogInformation(
                    "Order dispatched: run={RunId} account={AccountId} isin={Isin} side={Side} brokerOrderId={BrokerOrderId}",
                    intent.RunId, intent.TraderAccountId, intent.Isin, intent.Side, result.OrderId);
            }

            return brokerOrder;
        }
        catch (Exception ex)
        {
            brokerOrder.ApplyStateUpdate(OrderStateKind.Rejected, Quantity.Zero);
            intent.MarkFailed(ex.Message);

            _logger.LogError(ex,
                "Order dispatch failed: run={RunId} isin={Isin}",
                intent.RunId, intent.Isin);

            throw;
        }
    }
}