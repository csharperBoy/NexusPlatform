using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Infrastructure.Execution;

public sealed class OrderIntentBuilder : IOrderIntentBuilder
{
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ICapitalAllocator _capitalAllocator;
    private readonly IPositionSizer _positionSizer;
    private readonly ITraderUnitOfWork<Data.TraderDbContext> _uow;
    private readonly ILogger<OrderIntentBuilder> _logger;

    public OrderIntentBuilder(
        IMarketSnapshotStore snapshotStore,
        ICapitalAllocator capitalAllocator,
        IPositionSizer positionSizer,
        ITraderUnitOfWork<Data.TraderDbContext> uow,
        ILogger<OrderIntentBuilder> logger)
    {
        _snapshotStore = snapshotStore;
        _capitalAllocator = capitalAllocator;
        _positionSizer = positionSizer;
        _uow = uow;
        _logger = logger;
    }

    public async Task<OrderIntent?> BuildAsync(
        Signal signal,
        RunRuntimeContext context,
        Guid accountId,
        CancellationToken ct = default)
    {
        if (signal.Action == SignalAction.Hold)
            return null;

        // ─── Snapshot ───
        var snapshot = _snapshotStore.GetLatest(signal.Isin);
        if (snapshot is null)
        {
            _logger.LogWarning(
                "No snapshot for {Isin}; cannot build OrderIntent.", signal.Isin);
            return null;
        }

        // ─── تخصیص سرمایه به این Symbol ───
        var symbolAllocation = context.Symbols
            .FirstOrDefault(s => s.Isin == signal.Isin)?.Allocation;
        if (symbolAllocation is null || symbolAllocation.Value.Percentage <= 0m)
        {
            _logger.LogWarning(
                "No allocation for {Isin} in run {RunId}.", signal.Isin, context.RunId);
            return null;
        }

        var runCapital = await _capitalAllocator.GetAllocatableCapitalAsync(
            context, accountId, ct);

        if (runCapital <= 0m)
        {
            _logger.LogDebug(
                "Run {RunId} has no allocatable capital for account {AccountId}.",
                context.RunId, accountId);
            return null;
        }

        var allocatedCapital = runCapital * symbolAllocation.Value.Percentage / 100m;

        // ─── Position Sizing ───
        var quantity = _positionSizer.CalculateQuantity(
            snapshot, signal.Action, allocatedCapital);

        if (quantity <= 0)
        {
            _logger.LogDebug(
                "Calculated quantity is zero for {Isin}. Skipping.", signal.Isin);
            return null;
        }

        // ─── Price (Limit) ───
        var price = ResolvePrice(snapshot, signal.Action);

        // ─── SL/TP ───
        var entryPrice = new Price(price);

        var intent = OrderIntent.Create(
            runId: context.RunId,
            traderAccountId: accountId,
            sourceSignalId: signal.Id,
            isin: signal.Isin,
            side: signal.Action == SignalAction.Buy ? OrderSide.Buy : OrderSide.Sell,
            price: entryPrice,
            quantity: new Quantity(quantity),
            orderMode: context.DefaultOrderMode,
            reason: $"Signal {signal.Id} strength={signal.Strength.Value:0.##}");

        await _uow.OrderIntents.AddAsync(intent);

        _logger.LogInformation(
            "OrderIntent built: run={RunId} account={AccountId} isin={Isin} side={Side} qty={Qty} price={Price}",
            context.RunId, accountId, signal.Isin, intent.Side, quantity, price);

        return intent;
    }

    private static decimal ResolvePrice(MarketSnapshot snapshot, SignalAction action)
    {
        // Buy: از BestAskPrice اگر موجود باشد، وگرنه LastTradedPrice
        // Sell: از BestBuyPrice اگر موجود باشد، وگرنه LastTradedPrice
        if (action == SignalAction.Buy)
        {
            if (snapshot.BestSellPrice > 0) return snapshot.BestSellPrice;
            return snapshot.LastTradedPrice > 0 ? snapshot.LastTradedPrice : 0m;
        }

        if (snapshot.BestBuyPrice > 0) return snapshot.BestBuyPrice;
        return snapshot.LastTradedPrice > 0 ? snapshot.LastTradedPrice : 0m;
    }
}