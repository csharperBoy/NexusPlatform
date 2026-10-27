using Core.Application.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Positions;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Specifications.OrderIntents;
using Trader.Application.Specifications.Positions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Positions;

public sealed class PositionCloser : IPositionCloser
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PositionCloser> _logger;

    public PositionCloser(
        IServiceScopeFactory scopeFactory,
        ILogger<PositionCloser> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    public async Task<int> CloseRunPositionsAsync(
        Guid runId, string reason, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var uow = sp.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var spec = new OpenPositionsByRunSpec(runId);
        var positions = (await uow.PositionSpecs.ListBySpecAsync(spec)).ToList();

        if (positions.Count == 0)
        {
            _logger.LogDebug(
                "No open positions for run {RunId} to close.", runId);
            return 0;
        }

        await SetUserContextAsync(sp, runId, ct);
        return await CloseInternalAsync(sp, positions, reason, ct);
    }
    public async Task<int> ClosePositionsAsync(
    IReadOnlyList<Guid> positionIds, string reason,
    CancellationToken ct = default)
    {
        if (positionIds.Count == 0) return 0;

        using var scope = _scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;

        var uow = sp.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var positions = new List<Position>(positionIds.Count);
        foreach (var id in positionIds)
        {
            var pos = await uow.Positions.GetByIdAsync(id);
            if (pos is not null && pos.State != PositionState.Closed)
                positions.Add(pos);
        }

        if (positions.Count == 0) return 0;

        await SetUserContextAsync(sp, positions[0].RunId, ct);

        return await CloseInternalAsync(sp, positions, reason, ct);
    }

    /* ═══════════════════ Core ═══════════════════ */

    private async Task<int> CloseInternalAsync(
        IServiceProvider sp,
        IReadOnlyList<Position> positions,
        string reason,
        CancellationToken ct)
    {
        var uow = sp.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var snapshotStore = sp.GetRequiredService<IMarketSnapshotStore>();
        var sessionProvider = sp.GetRequiredService<IBrokerSessionProvider>();
        var dispatcher = sp.GetRequiredService<IBrokerOrderDispatcher>();

        var closed = 0;

        foreach (var pos in positions)
        {
            try
            {
                // ─── Skip اگر Sell باز داریم ───
                var existingSpec = new OpenSellIntentsByPositionSpec(
                    pos.RunId, pos.TraderAccountId, pos.Isin);
                var existing = (await uow.OrderIntentSpecs.ListBySpecAsync(existingSpec)).ToList();

                if (existing.Count > 0)
                {
                    _logger.LogDebug(
                        "Sell already pending for position {PositionId} ({Isin}); skipping.",
                        pos.Id, pos.Isin);
                    continue;
                }

                // ─── Snapshot ───
                var snap = snapshotStore.GetLatest(pos.Isin);
                if (snap is null)
                {
                    _logger.LogWarning(
                        "No snapshot for {Isin}; cannot close position {PositionId}.",
                        pos.Isin, pos.Id);
                    continue;
                }

                // ─── قیمت فروش: Best Bid (فروش سریع) ───
                decimal sellPriceValue = snap.BestBuyPrice > 0
                    ? snap.BestBuyPrice
                    : snap.LastTradedPrice;

                if (sellPriceValue <= 0m)
                {
                    _logger.LogWarning(
                        "No valid sell price for {Isin}; skipping.", pos.Isin);
                    continue;
                }

                var sellPrice = new Price(sellPriceValue);

                // ─── Account ───
                var account = await uow.Accounts.GetByIdAsync(pos.TraderAccountId);
                if (account is null) continue;

                // ─── OrderIntent ───
                var intent = OrderIntent.Create(
                    runId: pos.RunId,
                    traderAccountId: pos.TraderAccountId,
                    sourceSignalId: null,
                    isin: pos.Isin,
                    side: OrderSide.Sell,
                    price: sellPrice,
                    quantity: pos.Quantity,
                    orderMode: OrderMode.Quantity,
                    reason: reason);

                await uow.OrderIntents.AddAsync(intent);

                // ─── Dispatch ───
                var session = await sessionProvider.GetSessionAsync(
                    pos.TraderAccountId, ct);

                await dispatcher.DispatchAsync(intent, account.Broker, session, ct);
                closed++;

                _logger.LogInformation(
                    "Close dispatched: run={RunId} isin={Isin} qty={Qty} price={Price} reason={Reason}",
                    pos.RunId, pos.Isin, pos.Quantity.Value,
                    sellPriceValue, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to close position {PositionId} ({Isin}).",
                    pos.Id, pos.Isin);
            }
        }

        if (closed > 0)
        {
            await uow.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "Closed {Closed}/{Total} positions. Reason: {Reason}",
            closed, positions.Count, reason);

        return closed;
    }
    private async Task SetUserContextAsync(
    IServiceProvider sp, Guid runId, CancellationToken ct)
    {
        try
        {
            var runCache = sp.GetRequiredService<IRunExecutionContextCache>();
            var runCtx = runCache.Get(runId);

            if (runCtx is null)
            {
                _logger.LogWarning(
                    "Run {RunId} not in cache; user context will not be set. " +
                    "Permission checks may fail.", runId);
                return;
            }

            var userContext = sp.GetRequiredService<UserDataContext>();
            var userContextCache = sp.GetRequiredService<IUserContextCache>();

            var userCtx = await userContextCache.GetOrLoadAsync(
                runCtx.OwnerUserId, ct);

            userContext.ApplyFrom(userCtx);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to set user context for close operation. Continuing.");
        }
    }
}