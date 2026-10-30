using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Instruments;
using Trader.Application.Specifications.Positions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Infrastructure.Execution;

public sealed class OrderIntentBuilder : IOrderIntentBuilder
{
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ICapitalAllocator _capitalAllocator;
    private readonly ICommittedCapitalCalculator _committedCapital;
    private readonly ITraderUnitOfWork<Data.TraderDbContext> _uow;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<OrderIntentBuilder> _logger;

    public OrderIntentBuilder(
        IMarketSnapshotStore snapshotStore,
        ICapitalAllocator capitalAllocator,
        ICommittedCapitalCalculator committedCapital,
        ITraderUnitOfWork<Data.TraderDbContext> uow,
        ITradingEventLogger eventLogger,
        ILogger<OrderIntentBuilder> logger)
    {
        _snapshotStore = snapshotStore;
        _capitalAllocator = capitalAllocator;
        _committedCapital = committedCapital;
        _uow = uow;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task<OrderIntent?> BuildAsync(
        Signal signal,
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default)
    {
        if (signal.Action == SignalAction.Hold) return null;

        // ─── ۱. Snapshot ───
        var snapshot = _snapshotStore.GetLatest(signal.Isin);
        if (snapshot is null)
        {
            _logger.LogWarning("No snapshot for {Isin}.", signal.Isin);
            return null;
        }

        // ─── ۲. Instrument ───
        var instrument = await _uow.InstrumentSpecs.GetBySpecAsync(
            new InstrumentByIsinSpec(signal.Isin));

        if (instrument is null)
        {
            _logger.LogWarning("Instrument {Isin} not found.", signal.Isin);
            return null;
        }

        // ─── ۳. Allocation ───
        var symbolAllocation = context.Symbols
            .FirstOrDefault(s => s.Isin == signal.Isin)?.Allocation;
        if (symbolAllocation is null || symbolAllocation.Value.Percentage <= 0m)
        {
            _logger.LogDebug(
                "No allocation for {Isin} in run {RunId}.",
                signal.Isin, context.RunId);
            return null;
        }

        // ─── ۴. Price ───
        var price = ResolvePrice(snapshot, signal.Action);
        if (price <= 0m)
        {
            _logger.LogWarning("Invalid price for {Isin}.", signal.Isin);
            return null;
        }

        long quantity;

        if (signal.Action == SignalAction.Buy)
        {
            quantity = await ComputeBuyQuantityAsync(
                signal, context, accountId, instrument, price, symbolAllocation.Value, ct);

            if (quantity <= 0) return null;
        }
        else // Sell
        {
            quantity = await ComputeSellQuantityAsync(
                signal, context, accountId, instrument, ct);

            if (quantity <= 0) return null;
        }

        // ─── ۵. SL/TP (فقط BUY) ───
        var entryPrice = new Price(price);
        Price? sl = null;
        Price? tp = null;

        if (signal.Action == SignalAction.Buy)
        {
            sl = context.StopLoss?.ResolveForEntry(entryPrice);
            tp = context.TakeProfit?.ResolveForEntry(entryPrice);
        }

        // ─── ۶. Create Intent ───
        var intent = OrderIntent.Create(
            runId: context.RunId,
            traderAccountId: accountId,
            sourceSignalId: signal.Id,
            isin: signal.Isin,
            side: signal.Action == SignalAction.Buy ? OrderSide.Buy : OrderSide.Sell,
            price: entryPrice,
            quantity: new Quantity(quantity),
            orderMode: context.DefaultOrderMode,
            reason: $"Signal {signal.Id} strength={signal.Strength.Value:0.##}",
            stopLossPrice: sl,
            takeProfitPrice: tp,
            sourceEvaluationId: signal.SourceEvaluationId);

        await _uow.OrderIntents.AddAsync(intent);

        _logger.LogInformation(
            "OrderIntent built: run={RunId} isin={Isin} side={Side} qty={Qty} price={Price} SL={SL} TP={TP}",
            context.RunId, signal.Isin, intent.Side, quantity, price,
            sl?.Value, tp?.Value);

        return intent;
    }

    /* ═══════════════════ BUY ═══════════════════ */

    private async Task<long> ComputeBuyQuantityAsync(
        Signal signal,
        RunRuntimeContext context,
        Guid accountId,
        Instrument instrument,
        decimal price,
        SymbolAllocation allocation,
        CancellationToken ct)
    {
        // ─── ۱. سرمایه‌ی کل Run برای این Account ───
        var totalCapital = await _capitalAllocator.GetAllocatableCapitalAsync(
            context, accountId, ct);

        if (totalCapital <= 0m) return 0;

        var allocatedForSymbol = totalCapital * allocation.Percentage / 100m;

        // ─── ۲. Target با Stepped (strength-based) ───
        var targetCapital = allocatedForSymbol
            * signal.Strength.Value
            * context.StrengthMultiplier;

        if (targetCapital > allocatedForSymbol)
            targetCapital = allocatedForSymbol;   // cap

        if (targetCapital <= 0m) return 0;

        // ─── ۳. Committed ───
        var committed = await _committedCapital.CalculateForSymbolAsync(
            context.RunId, accountId, signal.Isin, ct);

        var delta = targetCapital - committed;
        if (delta <= 0m)
        {
            _logger.LogDebug(
                "Stepped BUY skipped: target={Target:N0} committed={Committed:N0} isin={Isin}",
                targetCapital, committed, signal.Isin);
            return 0;
        }
        // ─── MinDelta Threshold ───
        var minDelta = allocatedForSymbol * context.MinDeltaPercent;
        if (delta < minDelta)
        {
            _logger.LogDebug(
                "Stepped BUY skipped: delta={Delta:N0} < minDelta={MinDelta:N0} " +
                "(MinDeltaPercent={Pct:P2}) isin={Isin}",
                delta, minDelta, context.MinDeltaPercent, signal.Isin);
            return 0;
        }
        // ─── ۴. Commission ───
        var effectiveDelta = delta * (1m - instrument.BuyCommissionRate);

        // ─── ۵. Quantity ───
        var qty = (long)Math.Floor(effectiveDelta / price);

        // ─── ۶. Min value check ───
        if (instrument.MinOrderValueRial.HasValue)
        {
            var orderValue = qty * (long)price;
            if (orderValue < instrument.MinOrderValueRial.Value)
            {
                _logger.LogInformation(
                    "BUY skipped: orderValue={Value:N0} < minValue={Min:N0} isin={Isin}",
                    orderValue, instrument.MinOrderValueRial.Value, signal.Isin);
                return 0;
            }
        }

        // ─── ۷. Min/Max quantity ───
        if (qty < instrument.MinOrderQuantity)
        {
            _logger.LogInformation(
                "BUY skipped: qty={Qty} < minQty={Min} isin={Isin}",
                qty, instrument.MinOrderQuantity, signal.Isin);
            return 0;
        }

        if (qty > instrument.MaxOrderQuantity)
        {
            _logger.LogInformation(
                "BUY capped: requestedQty={Qty} > maxQty={Max} isin={Isin} (will split across ticks)",
                qty, instrument.MaxOrderQuantity, signal.Isin);
            qty = instrument.MaxOrderQuantity;
        }

        return qty;
    }

    /* ═══════════════════ SELL ═══════════════════ */

    private async Task<long> ComputeSellQuantityAsync(
        Signal signal,
        RunRuntimeContext context,
        Guid accountId,
        Instrument instrument,
        CancellationToken ct)
    {
        // ─── ۱. Position ───
        var positions = (await _uow.PositionSpecs.ListBySpecAsync(
            new OpenPositionsByRunSpec(context.RunId)))
            .Where(p => p.TraderAccountId == accountId
                     && p.Isin == signal.Isin
                     && p.State != PositionState.Closed)
            .ToList();

        var position = positions.FirstOrDefault();
        if (position is null)
        {
            _logger.LogDebug(
                "SELL skipped: no open position isin={Isin}", signal.Isin);
            return 0;
        }

        var qty = position.Quantity.Value;

        // ─── ۲. Min/Max ───
        if (qty < instrument.MinOrderQuantity)
        {
            _logger.LogInformation(
                "SELL skipped: positionQty={Qty} < minQty={Min} isin={Isin}",
                qty, instrument.MinOrderQuantity, signal.Isin);
            return 0;
        }

        if (qty > instrument.MaxOrderQuantity)
        {
            _logger.LogInformation(
                "SELL capped: positionQty={Qty} > maxQty={Max} isin={Isin} (will split across ticks)",
                qty, instrument.MaxOrderQuantity, signal.Isin);
            qty = instrument.MaxOrderQuantity;
        }

        return qty;
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private static decimal ResolvePrice(MarketSnapshot snapshot, SignalAction action)
    {
        if (action == SignalAction.Buy)
        {
            if (snapshot.BestSellPrice > 0) return snapshot.BestSellPrice;
            return snapshot.LastTradedPrice > 0 ? snapshot.LastTradedPrice : 0m;
        }

        if (snapshot.BestBuyPrice > 0) return snapshot.BestBuyPrice;
        return snapshot.LastTradedPrice > 0 ? snapshot.LastTradedPrice : 0m;
    }
}