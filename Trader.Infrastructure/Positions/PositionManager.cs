using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Positions;
using Trader.Application.Specifications.Positions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.Events;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Positions;

public sealed class PositionManager : IPositionManager
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<PositionManager> _logger;

    public PositionManager(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<PositionManager> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task ApplyTradeAsync(
        BrokerOrder order, OrderTrade trade, CancellationToken ct = default)
    {
        if (trade.Quantity.IsZero) return;

        var position = await FindPositionAsync(order, ct);


        if (order.Side == OrderSide.Buy)
        {
            if (position is null)
            {
                // ─── خواندن SL/TP از OrderIntent ───
                Price? sl = null;
                Price? tp = null;

                if (order.OrderIntentId.HasValue)
                {
                    var intent = await _uow.OrderIntents.GetByIdAsync(
                        order.OrderIntentId.Value);

                    sl = intent?.StopLossPrice;
                    tp = intent?.TakeProfitPrice;
                }

                position = Position.Open(
                    runId: order.RunId,
                    traderAccountId: order.TraderAccountId,
                    isin: trade.Isin,
                    quantity: trade.Quantity,
                    entryPrice: trade.Price,
                    stopLossPrice: sl,
                    takeProfitPrice: tp);

                await _uow.Positions.AddAsync(position);

                _logger.LogInformation(
                    "Position opened: run={RunId} account={AccountId} isin={Isin} qty={Qty} price={Price} SL={SL} TP={TP} trade={TradeNumber}",
                    order.RunId, order.TraderAccountId, trade.Isin,
                    trade.Quantity.Value, trade.Price.Value,
                    sl?.Value, tp?.Value, trade.TradeNumber);
            }
            else
            {
                position.ApplyBuyFill(trade.Quantity, trade.Price);

                _logger.LogInformation(
                    "Position increased: run={RunId} isin={Isin} +qty={Qty} price={Price} newAvg={Avg} trade={TradeNumber}",
                    order.RunId, trade.Isin, trade.Quantity.Value,
                    trade.Price.Value, position.AverageEntryPrice.Value,
                    trade.TradeNumber);
            }
        }
        else // Sell
        {
            if (position is null)
            {
                _logger.LogWarning(
                    "Sell trade without open position: run={RunId} isin={Isin} qty={Qty} trade={TradeNumber}",
                    order.RunId, trade.Isin, trade.Quantity.Value, trade.TradeNumber);
                return;
            }

            var sellQty = trade.Quantity.Value > position.Quantity.Value
                ? position.Quantity.Value
                : trade.Quantity.Value;

            if (sellQty < trade.Quantity.Value)
            {
                _logger.LogWarning(
                    "Sell trade {TradeQty} exceeds position {PosQty} for {Isin}; clamping to {Clamped}.",
                    trade.Quantity.Value, position.Quantity.Value, trade.Isin, sellQty);
            }

            position.ApplySellFill(new Quantity(sellQty), trade.Price);

            _logger.LogInformation(
                "Position reduced: run={RunId} isin={Isin} -qty={Qty} price={Price} newState={State} trade={TradeNumber}",
                order.RunId, trade.Isin, sellQty,
                trade.Price.Value, position.State, trade.TradeNumber);
        }

        position.AddDomainEvent(new PositionChangedEvent(
            position.Id,
            position.RunId,
            position.TraderAccountId,
            position.Isin,
            position.State,
            position.Quantity.Value,
            position.AverageEntryPrice.Value,
            DateTime.UtcNow));
    }

    public async Task ApplyTradeCancelAsync(
        BrokerOrder order, OrderTrade trade, CancellationToken ct = default)
    {
        var position = await FindPositionAsync(order, ct);
        if (position is null)
        {
            _logger.LogWarning(
                "Trade canceled but no open position: run={RunId} isin={Isin} trade={TradeNumber}",
                order.RunId, trade.Isin, trade.TradeNumber);
            return;
        }

        // کنسلی Trade = معکوس کردن اثر آن
        if (trade.Side == OrderSide.Buy)
        {
            // Buy Cancel → کاهش Position
            var reduceQty = trade.Quantity.Value > position.Quantity.Value
                ? position.Quantity.Value
                : trade.Quantity.Value;

            if (reduceQty > 0)
            {
                // Cancel Buy یعنی این Buy اتفاق نیفتاده. پس Position باید برگردد.
                // بهترین رویکرد: کاهش Quantity بدون PnL (چون فروش نبوده).
                position.ApplyBuyCancel(new Quantity(reduceQty), trade.Price);

                _logger.LogWarning(
                    "Buy trade canceled: run={RunId} isin={Isin} -qty={Qty} trade={TradeNumber}",
                    order.RunId, trade.Isin, reduceQty, trade.TradeNumber);
            }
        }
        else
        {
            // Sell Cancel → افزودن Position برمی‌گردد
            position.ApplySellCancel(trade.Quantity, trade.Price);

            _logger.LogWarning(
                "Sell trade canceled: run={RunId} isin={Isin} +qty={Qty} trade={TradeNumber}",
                order.RunId, trade.Isin, trade.Quantity.Value, trade.TradeNumber);
        }
    }

    private async Task<Position?> FindPositionAsync(
        BrokerOrder order, CancellationToken ct)
    {
        var spec = new OpenPositionsByRunSpec(order.RunId);
        var openPositions = await _uow.PositionSpecs.ListBySpecAsync(spec);

        return openPositions.FirstOrDefault(p =>
            p.Isin == order.Isin
            && p.TraderAccountId == order.TraderAccountId);
    }
}