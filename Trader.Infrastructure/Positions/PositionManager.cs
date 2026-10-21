using System.Text.Json;
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

        if (trade.Side == OrderSide.Buy)
        {
            if (position is null)
            {
                // ─── بار اول: خواندن SL/TP و SourceEvaluationId از OrderIntent ───
                Price? sl = null;
                Price? tp = null;
                Guid? sourceEvalId = null;

                if (order.OrderIntentId.HasValue)
                {
                    var intent = await _uow.OrderIntents.GetByIdAsync(
                        order.OrderIntentId.Value);
                    sl = intent?.StopLossPrice;
                    tp = intent?.TakeProfitPrice;
                    sourceEvalId = intent?.SourceEvaluationId;
                }

                position = Position.Open(
                    runId: order.RunId,
                    traderAccountId: order.TraderAccountId,
                    isin: trade.Isin,
                    quantity: trade.Quantity,
                    entryPrice: trade.Price,
                    stopLossPrice: sl,
                    takeProfitPrice: tp,
                    sourceEvaluationId: sourceEvalId);   // ← جدید

                await _uow.Positions.AddAsync(position);

                _logger.LogInformation(
                    "Position opened: run={RunId} account={AccountId} isin={Isin} " +
                    "qty={Qty} price={Price} SL={SL} TP={TP} evalId={EvalId} trade={TradeNumber}",
                    order.RunId, order.TraderAccountId, trade.Isin,
                    trade.Quantity.Value, trade.Price.Value,
                    sl?.Value, tp?.Value, sourceEvalId, trade.TradeNumber);
            }
            else
            {
                position.ApplyBuyFill(trade.Quantity, trade.Price);

                _logger.LogInformation(
                    "Position increased: run={RunId} isin={Isin} +qty={Qty} price={Price} newAvg={Avg}",
                    order.RunId, trade.Isin, trade.Quantity.Value,
                    trade.Price.Value, position.AverageEntryPrice.Value);
            }
        }
        else // Sell
        {
            if (position is null)
            {
                _logger.LogWarning(
                    "Sell trade without open position: run={RunId} isin={Isin} qty={Qty}",
                    order.RunId, trade.Isin, trade.Quantity.Value);
                return;
            }

            var sellQty = trade.Quantity.Value > position.Quantity.Value
                ? position.Quantity.Value
                : trade.Quantity.Value;

            if (sellQty < trade.Quantity.Value)
            {
                _logger.LogWarning(
                    "Sell trade {Qty} exceeds position {PosQty} for {Isin}; clamped to {Clamped}.",
                    trade.Quantity.Value, position.Quantity.Value, trade.Isin, sellQty);
            }

            position.ApplySellFill(new Quantity(sellQty), trade.Price);

            _logger.LogInformation(
                "Position reduced: run={RunId} isin={Isin} -qty={Qty} price={Price} newState={State}",
                order.RunId, trade.Isin, sellQty,
                trade.Price.Value, position.State);

            // ─── اگر Position بسته شد → Outcome ثبت کن ───
            if (position.State == PositionState.Closed
                && position.SourceEvaluationId.HasValue)
            {
                await UpdateEvaluationOutcomeAsync(position, order, trade, ct);
            }
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
        if (position is null) return;

        if (trade.Side == OrderSide.Buy)
        {
            var reduceQty = trade.Quantity.Value > position.Quantity.Value
                ? position.Quantity.Value
                : trade.Quantity.Value;
            if (reduceQty > 0)
                position.ApplyBuyCancel(new Quantity(reduceQty), trade.Price);
        }
        else
        {
            position.ApplySellCancel(trade.Quantity, trade.Price);
        }
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private async Task<Position?> FindPositionAsync(
        BrokerOrder order, CancellationToken ct)
    {
        var spec = new OpenPositionsByRunSpec(order.RunId);
        var positions = await _uow.PositionSpecs.ListBySpecAsync(spec);

        return positions.FirstOrDefault(p =>
            p.Isin == order.Isin
            && p.TraderAccountId == order.TraderAccountId);
    }

    private async Task UpdateEvaluationOutcomeAsync(
        Position position,
        BrokerOrder closingOrder,
        OrderTrade closingTrade,
        CancellationToken ct)
    {
        try
        {
            var eval = await _uow.StrategyEvaluations.GetByIdAsync(
                position.SourceEvaluationId!.Value);
            if (eval is null)
            {
                _logger.LogWarning(
                    "StrategyEvaluation {EvalId} not found for closed position {PosId}.",
                    position.SourceEvaluationId, position.Id);
                return;
            }

            // ─── تشخیص Exit Reason ───
            var exitReason = DetectExitReason(position, closingOrder);

            var entryPrice = position.AverageEntryPrice.Value;
            var exitPrice = closingTrade.Price.Value;
            var returnPct = entryPrice > 0
                ? (exitPrice - entryPrice) / entryPrice * 100m
                : 0m;
            var heldSeconds = (long)(DateTime.UtcNow - position.OpenedAt).TotalSeconds;

            var outcome = new
            {
                entryPrice,
                exitPrice,
                closedQuantity = closingTrade.Quantity.Value,
                realizedPnl = position.RealizedPnL.Amount,
                returnPct = Math.Round(returnPct, 4),
                exitReason,
                heldSeconds,
                openedAt = position.OpenedAt,
                closedAt = DateTime.UtcNow,
            };

            var json = JsonSerializer.Serialize(outcome, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            eval.UpdateOutcome(json);
            await _uow.StrategyEvaluations.UpdateAsync(eval);

            _logger.LogInformation(
                "Evaluation outcome recorded: evalId={EvalId} return={ReturnPct:0.##}% reason={Reason}",
                eval.Id, returnPct, exitReason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to record evaluation outcome for eval {EvalId}.",
                position.SourceEvaluationId);
        }
    }

    private static string DetectExitReason(
        Position position, BrokerOrder closingOrder)
    {
        // اولویت ۱: قیمت خروج نسبت به SL/TP
        if (position.StopLossPrice.HasValue
            && closingOrder.Price.Value <= position.StopLossPrice.Value.Value)
        {
            return "StopLoss";
        }

        if (position.TakeProfitPrice.HasValue
            && closingOrder.Price.Value >= position.TakeProfitPrice.Value.Value)
        {
            return "TakeProfit";
        }

        // اولویت ۲: از OrderIntent.Reason (اگر موجود)
        // (OrderIntent در اینجا Load نشده، پس در مرحله ۲ اضافه می‌کنیم)

        return "Manual";
    }
}