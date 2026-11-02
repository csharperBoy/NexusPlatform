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
    private readonly IOrderSizeCalculator _sizeCalculator;
    private readonly ITraderUnitOfWork<Data.TraderDbContext> _uow;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<OrderIntentBuilder> _logger;

    public OrderIntentBuilder(
        IMarketSnapshotStore snapshotStore,
        ICapitalAllocator capitalAllocator,
        ICommittedCapitalCalculator committedCapital,
        IOrderSizeCalculator sizeCalculator,
        ITraderUnitOfWork<Data.TraderDbContext> uow,
        ITradingEventLogger eventLogger,
        ILogger<OrderIntentBuilder> logger)
    {
        _snapshotStore = snapshotStore;
        _capitalAllocator = capitalAllocator;
        _committedCapital = committedCapital;
        _sizeCalculator = sizeCalculator;
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

        var snapshot = _snapshotStore.GetLatest(signal.Isin);
        if (snapshot is null)
        {
            _logger.LogWarning("No snapshot for {Isin}.", signal.Isin);
            return null;
        }

        var instrument = await _uow.InstrumentSpecs.GetBySpecAsync(
            new InstrumentByIsinSpec(signal.Isin));
        if (instrument is null)
        {
            _logger.LogWarning("Instrument {Isin} not found.", signal.Isin);
            return null;
        }

        var symbolAllocation = context.Symbols
            .FirstOrDefault(s => s.Isin == signal.Isin)?.Allocation;
        if (symbolAllocation is null || symbolAllocation.Value.Percentage <= 0m)
        {
            _logger.LogDebug("No allocation for {Isin}.", signal.Isin);
            return null;
        }

        var price = ResolvePrice(snapshot, signal.Action);
        if (price <= 0m)
        {
            _logger.LogWarning("Invalid price for {Isin}.", signal.Isin);
            return null;
        }

        long quantity;

        if (signal.Action == SignalAction.Buy)
        {
            var totalCapital = await _capitalAllocator.GetAllocatableCapitalAsync(
                context, accountId, ct);
            if (totalCapital <= 0m) return null;

            var allocatedForSymbol = totalCapital * symbolAllocation.Value.Percentage / 100m;

            var committed = await _committedCapital.CalculateForSymbolAsync(
                context.RunId, accountId, signal.Isin, ct);

            var result = _sizeCalculator.CalculateBuy(new BuySizeInput(
                AllocatedCapital: allocatedForSymbol,
                CommittedCapital: committed,
                AvailableCash: 0m,               // Live → broker check
                SignalStrength: signal.Strength.Value,
                StrengthMultiplier: context.StrengthMultiplier,
                MinDeltaPercent: context.MinDeltaPercent,
                Price: price,
                BuyCommissionRate: instrument.BuyCommissionRate,
                MinOrderQuantity: instrument.MinOrderQuantity,
                MaxOrderQuantity: instrument.MaxOrderQuantity,
                MinOrderValueRial: instrument.MinOrderValueRial));

            if (result.Quantity <= 0)
            {
                _logger.LogDebug(
                    "BUY skipped for {Isin}: {Reason} (target={Target:N0} committed={Committed:N0} delta={Delta:N0})",
                    signal.Isin, result.SkipReason,
                    result.TargetCapital, result.CommittedCapital, result.Delta);
                return null;
            }

            quantity = result.Quantity;
        }
        else
        {
            var positions = (await _uow.PositionSpecs.ListBySpecAsync(
                new OpenPositionsByRunSpec(context.RunId)))
                .Where(p => p.TraderAccountId == accountId
                         && p.Isin == signal.Isin
                         && p.State != PositionState.Closed)
                .ToList();

            var position = positions.FirstOrDefault();
            if (position is null)
            {
                _logger.LogDebug("SELL skipped: no position for {Isin}.", signal.Isin);
                return null;
            }

            var result = _sizeCalculator.CalculateSell(new SellSizeInput(
                PositionQuantity: position.Quantity.Value,
                MinOrderQuantity: instrument.MinOrderQuantity,
                MaxOrderQuantity: instrument.MaxOrderQuantity));

            if (result.Quantity <= 0)
            {
                _logger.LogDebug("SELL skipped for {Isin}: {Reason}",
                    signal.Isin, result.SkipReason);
                return null;
            }

            quantity = result.Quantity;
        }

        var entryPrice = new Price(price);
        Price? sl = null;
        Price? tp = null;

        if (signal.Action == SignalAction.Buy)
        {
            sl = context.StopLoss?.ResolveForEntry(entryPrice);
            tp = context.TakeProfit?.ResolveForEntry(entryPrice);
        }

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
            "OrderIntent built: run={RunId} isin={Isin} side={Side} qty={Qty} price={Price}",
            context.RunId, signal.Isin, intent.Side, quantity, price);

        return intent;
    }

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