using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Positions;
using Trader.Application.Abstractions.TradingRuns;
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
    private readonly ITradingEventLogger _eventLogger;
    private readonly IRunExecutionContextCache _runCache;
    private readonly ILogger<PositionManager> _logger;

    public PositionManager(
        ITraderUnitOfWork<TraderDbContext> uow,
        ITradingEventLogger eventLogger,
        IRunExecutionContextCache runCache,
        ILogger<PositionManager> logger)
    {
        _uow = uow;
        _eventLogger = eventLogger;
        _runCache = runCache;
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
    sourceEvaluationId: sourceEvalId,
    isPaper: order.IsPaper);                        // ← NEW

                await _uow.Positions.AddAsync(position);

                _logger.LogInformation(
                    "Position opened: run={RunId} account={AccountId} isin={Isin} " +
                    "qty={Qty} price={Price} SL={SL} TP={TP} evalId={EvalId} trade={TradeNumber}",
                    order.RunId, order.TraderAccountId, trade.Isin,
                    trade.Quantity.Value, trade.Price.Value,
                    sl?.Value, tp?.Value, sourceEvalId, trade.TradeNumber);

                // ─── PositionOpened Event ───
                _eventLogger.Log(
                    stage: TradingStage.OrderLifecycle,
                    type: TradingEventType.PositionOpened,
                    severity: TradingEventSeverity.Info,
                    message: $"موقعیت باز شد: {trade.Quantity.Value:N0} سهم «{trade.Isin}» " +
                             $"به قیمت {trade.Price.Value:N0} ریال" +
                             (sl.HasValue ? $" (SL: {sl.Value.Value:N0})" : "") +
                             (tp.HasValue ? $" (TP: {tp.Value.Value:N0})" : ""),
                    payload: new
                    {
                        quantity = trade.Quantity.Value,
                        entryPrice = trade.Price.Value,
                        stopLossPrice = sl?.Value,
                        takeProfitPrice = tp?.Value,
                        tradeNumber = trade.TradeNumber,
                    },
                    runId: order.RunId,
                    isin: trade.Isin,
                    accountId: order.TraderAccountId,
                    brokerOrderId: order.Id,
                    tradeId: trade.Id,
                    positionId: position.Id);
            }
            else
            {
                var addedQty = trade.Quantity.Value;
                var fillPrice = trade.Price.Value;

                position.ApplyBuyFill(trade.Quantity, trade.Price);

                _logger.LogInformation(
                    "Position increased: run={RunId} isin={Isin} +qty={Qty} price={Price} newAvg={Avg}",
                    order.RunId, trade.Isin, addedQty, fillPrice,
                    position.AverageEntryPrice.Value);

                // ─── PositionIncreased Event ───
                _eventLogger.Log(
                    stage: TradingStage.OrderLifecycle,
                    type: TradingEventType.PositionIncreased,
                    severity: TradingEventSeverity.Info,
                    message: $"موقعیت افزایش یافت: +{addedQty:N0} سهم «{trade.Isin}» " +
                             $"به قیمت {fillPrice:N0} (میانگین جدید: {position.AverageEntryPrice.Value:N0})",
                    payload: new
                    {
                        addedQuantity = addedQty,
                        fillPrice = fillPrice,
                        newAverageEntryPrice = position.AverageEntryPrice.Value,
                        totalQuantity = position.Quantity.Value,
                        tradeNumber = trade.TradeNumber,
                    },
                    runId: order.RunId,
                    isin: trade.Isin,
                    accountId: order.TraderAccountId,
                    brokerOrderId: order.Id,
                    tradeId: trade.Id,
                    positionId: position.Id);
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

            // ═══════════════════════════════════════════════════════════
            // CRITICAL: Position Overshoot Detection
            //
            // اگر Broker یک Sell بزرگ‌تر از Position ثبت‌شده گزارش دهد،
            // State Domain با واقعیت همخوان نیست. علت‌های ممکن:
            //   • Manual Trade در Broker (خارج از سیستم)
            //   • Trade از Account دیگری (data bug)
            //   • Data Race در ترتیب پردازش
            //   • Position پس از Close شدن، دوباره Sell شده
            //
            // تصمیم: Fill را اعمال نمی‌کنیم (Position در "known good"
            // state می‌ماند)، Run را Fault می‌کنیم، و از Cache خارج
            // می‌کنیم. Reconciliation اختلاف را نشان می‌دهد.
            // ═══════════════════════════════════════════════════════════
            if (trade.Quantity.Value > position.Quantity.Value)
            {
                var overshootQty = trade.Quantity.Value - position.Quantity.Value;

                _logger.LogCritical(
                    "POSITION OVERSHOOT DETECTED: broker sell trade {TradeQty} " +
                    "exceeds tracked position {PosQty} for {Isin} " +
                    "(run={RunId}, account={AccountId}, trade={TradeNumber}). " +
                    "Fill NOT applied. Run will be faulted.",
                    trade.Quantity.Value, position.Quantity.Value, trade.Isin,
                    order.RunId, order.TraderAccountId, trade.TradeNumber);

                _eventLogger.Log(
                    stage: TradingStage.OrderLifecycle,
                    type: TradingEventType.PositionOvershootDetected,
                    severity: TradingEventSeverity.Critical,
                    message: $"⚠️ مغایرت بحرانی: Broker فروش {trade.Quantity.Value:N0} سهم " +
                             $"«{trade.Isin}» را گزارش داد، ولی Position ثبت‌شده فقط " +
                             $"{position.Quantity.Value:N0} سهم دارد. " +
                             $"مقدار اضافی: {overshootQty:N0} سهم. " +
                             $"Fill اعمال نشد. پلن برای بررسی دستی Fault شد.",
                    payload: new
                    {
                        brokerSellQuantity = trade.Quantity.Value,
                        positionQuantity = position.Quantity.Value,
                        overshootQuantity = overshootQty,
                        tradeNumber = trade.TradeNumber,
                        tradePrice = trade.Price.Value,
                        tradeDate = trade.TradeDate,
                    },
                    runId: order.RunId,
                    isin: trade.Isin,
                    accountId: order.TraderAccountId,
                    brokerOrderId: order.Id,
                    tradeId: trade.Id,
                    positionId: position.Id);

                await FaultRunAsync(order.RunId, ct);

                // Fill اعمال نشد:
                // - Trade record در DB هست (audit trail)
                // - FilledQuantity توسط caller (SyncTradesAsync) به‌روز می‌شود
                // - Position در "known good" state می‌ماند
                // - Reconciliation اختلاف Position vs Broker را نشان می‌دهد
                return;
            }

            // ═══ Normal sell path ═══
            var sellQty = trade.Quantity.Value;
            var fillPrice = trade.Price.Value;
            var pnlBefore = position.RealizedPnL.Amount;

            position.ApplySellFill(new Quantity(sellQty), trade.Price);

            _logger.LogInformation(
                "Position reduced: run={RunId} isin={Isin} -qty={Qty} price={Price} newState={State}",
                order.RunId, trade.Isin, sellQty, fillPrice, position.State);

            var pnlDelta = position.RealizedPnL.Amount - pnlBefore;

            if (position.State == PositionState.Closed)
            {
                // ─── PositionClosed Event ───
                _eventLogger.Log(
                    stage: TradingStage.OrderLifecycle,
                    type: TradingEventType.PositionClosed,
                    severity: TradingEventSeverity.Info,
                    message: $"موقعیت بسته شد: {sellQty:N0} سهم «{trade.Isin}» " +
                             $"به قیمت {fillPrice:N0}. " +
                             $"سود/زیان محقق: {pnlDelta:N0} ریال",
                    payload: new
                    {
                        closedQuantity = sellQty,
                        exitPrice = fillPrice,
                        realizedPnl = pnlDelta,
                        totalRealizedPnl = position.RealizedPnL.Amount,
                        tradeNumber = trade.TradeNumber,
                    },
                    runId: order.RunId,
                    isin: trade.Isin,
                    accountId: order.TraderAccountId,
                    brokerOrderId: order.Id,
                    tradeId: trade.Id,
                    positionId: position.Id);

                if (position.SourceEvaluationId.HasValue)
                    await UpdateEvaluationOutcomeAsync(position, order, trade, ct);
            }
            else
            {
                // ─── PositionReduced Event ───
                _eventLogger.Log(
                    stage: TradingStage.OrderLifecycle,
                    type: TradingEventType.PositionReduced,
                    severity: TradingEventSeverity.Info,
                    message: $"موقعیت کاهش یافت: -{sellQty:N0} سهم «{trade.Isin}» " +
                             $"به قیمت {fillPrice:N0}. " +
                             $"سود/زیان محقق: {pnlDelta:N0} ریال " +
                             $"({position.Quantity.Value:N0} سهم باقی)",
                    payload: new
                    {
                        reducedQuantity = sellQty,
                        exitPrice = fillPrice,
                        realizedPnl = pnlDelta,
                        remainingQuantity = position.Quantity.Value,
                        tradeNumber = trade.TradeNumber,
                    },
                    runId: order.RunId,
                    isin: trade.Isin,
                    accountId: order.TraderAccountId,
                    brokerOrderId: order.Id,
                    tradeId: trade.Id,
                    positionId: position.Id);
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
    /// <summary>
    /// Run را به وضعیت Faulted منتقل می‌کند و از Runtime Cache خارج می‌کند
    /// تا Pipeline دیگر آن را پردازش نکند. Positionها دست‌نخورده می‌مانند
    /// (KeepPositions) و نیاز به بررسی دستی دارند.
    /// </summary>
    private async Task FaultRunAsync(Guid runId, CancellationToken ct)
    {
        try
        {
            var run = await _uow.TradingRuns.GetByIdAsync(runId);
            if (run is null)
            {
                _logger.LogError(
                    "Cannot fault run {RunId}: TradingRun not found in DB.", runId);
                // با این حال از Cache خارج کن — pipeline باید متوقف شود
                _runCache.Remove(runId);
                return;
            }

            if (run.State == TradingRunState.Running)
            {
                run.MarkFaulted(
                    "Position overshoot detected: broker reported sell trade " +
                    "exceeding tracked position quantity. Fill was NOT applied " +
                    "to avoid silent data corruption. Manual reconciliation required.");
                await _uow.TradingRuns.UpdateAsync(run);
                await _uow.SaveChangesAsync(ct);

                _logger.LogWarning(
                    "TradingRun {RunId} marked as Faulted due to position overshoot.",
                    runId);
            }
            else
            {
                _logger.LogWarning(
                    "Run {RunId} state is {State} (not Running); skipping MarkFaulted. " +
                    "Still removing from cache.",
                    runId, run.State);
            }

            // خارج کردن از Cache — Stageهای بعدی این Run را نمی‌بینند.
            // از TradingRunManager.StopAsync استفاده نمی‌کنیم چون آن
            // RunStopped event صادر می‌کند که برای Fault گمراه‌کننده است.
            _runCache.Remove(runId);
        }
        catch (Exception ex)
        {
            // هرگز نگذار Fault handling خودش Pipeline را بشکند.
            // حداقل Cache را پاک کن.
            _logger.LogError(ex,
                "Failed to fault run {RunId} after position overshoot. " +
                "Removing from cache anyway.", runId);

            try { _runCache.Remove(runId); }
            catch { /* swallow */ }
        }
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
        // ═══════════════ ۱. ChangeTracker (Positionهای Save نشده) ═══════════════
        var tracked = _uow.Context.ChangeTracker
            .Entries<Position>()
            .Where(e => e.State != EntityState.Deleted)
            .Select(e => e.Entity)
            .FirstOrDefault(p =>
                p.RunId == order.RunId
                && p.TraderAccountId == order.TraderAccountId
                && p.Isin == order.Isin
                && p.State != PositionState.Closed);

        if (tracked is not null)
            return tracked;

        // ═══════════════ ۲. DB (Positionهای Save شده) ═══════════════
        var spec = new OpenPositionsByRunSpec(order.RunId);
        var positions = (await _uow.PositionSpecs.ListBySpecAsync(spec)).ToList();

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

            // ─── EvaluationOutcomeRecorded Event ───
            _eventLogger.Log(
                stage: TradingStage.OrderLifecycle,
                type: TradingEventType.EvaluationOutcomeRecorded,
                severity: TradingEventSeverity.Info,
                message: $"نتیجه‌ی تحلیل ثبت شد: بازده {returnPct:0.##}% " +
                         $"(دلیل: {ReasonToPersian(exitReason)})",
                payload: new
                {
                    returnPct,
                    exitReason,
                    realizedPnl = position.RealizedPnL.Amount,
                    heldSeconds,
                },
                runId: position.RunId,
                isin: position.Isin,
                accountId: position.TraderAccountId,
                signalId: eval.SignalId,
                positionId: position.Id);
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

        return "Manual";
    }

    private static string ReasonToPersian(string reason) => reason switch
    {
        "StopLoss" => "حد ضرر",
        "TakeProfit" => "حد سود",
        "Manual" => "دستی",
        _ => reason
    };
}