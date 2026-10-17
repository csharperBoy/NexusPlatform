using Core.Application.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.Positions;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Dtos.Order;
using Trader.Application.Runtime;
using Trader.Application.Specifications.BrokerOrders;
using Trader.Application.Specifications.OrderTrades;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Runtime.Stages;

public sealed class OrderLifecycleStage : IOrderLifecycleStage
{
    private const int MaxPagesPerAccount = 10;
    private const int HistoryPageSize = 100;

    private DateTime _lastRunAt = DateTime.MinValue;

    private readonly IRunExecutionContextCache _runCache;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly IPositionManager _positionManager;
    private readonly IUserContextCache _userContextCache;
    private readonly UserDataContext _userContext;
    private readonly TradingRuntimeOptions _options;
    private readonly ILogger<OrderLifecycleStage> _logger;

    public OrderLifecycleStage(
        IRunExecutionContextCache runCache,
        ITraderUnitOfWork<TraderDbContext> uow,
        IBrokerSessionProvider sessionProvider,
        IBrokerClientFactory brokerFactory,
        IPositionManager positionManager,
        IUserContextCache userContextCache,
        UserDataContext userContext,
        IOptions<TradingRuntimeOptions> options,
        ILogger<OrderLifecycleStage> logger)
    {
        _runCache = runCache;
        _uow = uow;
        _sessionProvider = sessionProvider;
        _brokerFactory = brokerFactory;
        _positionManager = positionManager;
        _userContextCache = userContextCache;
        _userContext = userContext;
        _options = options.Value;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        if (now - _lastRunAt < _options.OrderLifecycleInterval)
            return;
        _lastRunAt = now;

        var contexts = _runCache.GetAllActive();
        if (contexts.Count == 0) return;

        var accountsWithRuns = contexts
            .SelectMany(c => c.AccountIds.Select(accId => (Run: c, AccountId: accId)))
            .GroupBy(x => x.AccountId)
            .ToList();

        foreach (var group in accountsWithRuns)
        {
            var accountId = group.Key;
            var anyRun = group.First().Run;

            try
            {
                var userCtx = await _userContextCache.GetOrLoadAsync(
                    anyRun.OwnerUserId, ct);
                _userContext.ApplyFrom(userCtx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to load user context for account {AccountId}. Skipping.",
                    accountId);
                continue;
            }

            try
            {
                await SyncAccountAsync(accountId, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Order lifecycle failed for account {AccountId}.", accountId);
            }
        }
    }

    /* ═══════════════════ Per Account ═══════════════════ */

    private async Task SyncAccountAsync(Guid accountId, CancellationToken ct)
    {
        var spec = new OpenBrokerOrdersByAccountSpec(accountId);
        var openOrders = (await _uow.BrokerOrderSpecs.ListBySpecAsync(spec))
            .Where(o => !o.IsTerminal)
            .Where(o => !string.IsNullOrWhiteSpace(o.BrokerOrderId))
            .ToList();

        if (openOrders.Count == 0) return;

        var account = await _uow.Accounts.GetByIdAsync(accountId);
        if (account is null) return;

        var session = await _sessionProvider.GetSessionAsync(accountId, ct);
        var historyReader = _brokerFactory.Get<IBrokerOrderHistoryReader>(account.Broker);

        // ─── ۱. Fetch تمام Order History (Pagination) ───
        var brokerOrders = await FetchAllHistoryAsync(historyReader, session, ct);
        if (brokerOrders.Count == 0) return;

        var brokerOrdersById = brokerOrders
            .GroupBy(o => o.OrderId)
            .ToDictionary(g => g.Key, g => g.Last());

        // ─── ۲. برای هر Order باز ما، وضعیت را Sync کن ───
        var modifiedCount = 0;

        foreach (var ourOrder in openOrders)
        {
            if (ourOrder.BrokerOrderId is null) continue;
            if (!brokerOrdersById.TryGetValue(ourOrder.BrokerOrderId, out var dto)) continue;

            try
            {
                var changed = await SyncOrderAsync(
                    ourOrder, dto, historyReader, session, ct);

                if (changed) modifiedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to sync order {OrderId} for account {AccountId}.",
                    ourOrder.BrokerOrderId, accountId);
            }
        }

        if (modifiedCount > 0)
        {
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation(
                "Stage 4: {Count} orders updated for account {AccountId}.",
                modifiedCount, accountId);
        }
    }

    /* ═══════════════════ Fetch History ═══════════════════ */

    private async Task<List<OrderHistoryItemDto>> FetchAllHistoryAsync(
        IBrokerOrderHistoryReader reader,
        Trader.ExternalApiClient.ExternalSystemSession session,
        CancellationToken ct)
    {
        var all = new List<OrderHistoryItemDto>();
        var page = 1;

        while (page <= MaxPagesPerAccount)
        {
            var query = new OrderHistoryQuery
            {
                Page = page,
                PageSize = HistoryPageSize
            };

            var result = await reader.GetOrderHistoryAsync(session, query, ct);
            if (result?.Items is null || result.Items.Count == 0) break;

            all.AddRange(result.Items);

            if (result.Items.Count < HistoryPageSize) break;
            page++;
        }

        return all;
    }

    /* ═══════════════════ Sync یک Order ═══════════════════ */

    private async Task<bool> SyncOrderAsync(
        BrokerOrder ourOrder,
        OrderHistoryItemDto dto,
        IBrokerOrderHistoryReader historyReader,
        Trader.ExternalApiClient.ExternalSystemSession session,
        CancellationToken ct)
    {
        var changed = false;

        // ─── ۱. Sync State ───
        if (dto.State != ourOrder.State)
        {
            ourOrder.ApplyStateUpdate(dto.State, ourOrder.FilledQuantity);
            changed = true;

            _logger.LogInformation(
                "Order state changed: {OrderId} {OldState} → {NewState}",
                ourOrder.BrokerOrderId, ourOrder.State, dto.State);
        }

        // ─── ۲. Sync Trades (اگر ExecutedQuantity افزایش یافته) ───
        if (dto.ExecutedQuantity > ourOrder.FilledQuantity.Value)
        {
            var tradeChanged = await SyncTradesAsync(
                ourOrder, historyReader, session, ct);
            changed = changed || tradeChanged;
        }

        return changed;
    }

    /* ═══════════════════ Sync Trades ═══════════════════ */

    private async Task<bool> SyncTradesAsync(
        BrokerOrder ourOrder,
        IBrokerOrderHistoryReader historyReader,
        Trader.ExternalApiClient.ExternalSystemSession session,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(ourOrder.BrokerOrderId)) return false;

        // ─── Fetch trades از Broker ───
        List<OrderTradeDto> brokerTrades;
        try
        {
            brokerTrades = await historyReader.GetOrderTradesAsync(
                session, ourOrder.BrokerOrderId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to fetch trades for order {OrderId}.",
                ourOrder.BrokerOrderId);
            return false;
        }

        if (brokerTrades is null || brokerTrades.Count == 0) return false;

        // ─── Trades موجود ما برای این Order ───
        var existingSpec = new TradesByBrokerOrderSpec(ourOrder.Id);
        var existingTrades = await _uow.OrderTradeSpecs.ListBySpecAsync(existingSpec);
        var existingByNumber = existingTrades.ToDictionary(t => t.TradeNumber);

        var newFilledQty = ourOrder.FilledQuantity.Value;
        var changed = false;

        foreach (var dto in brokerTrades)
        {
            try
            {
                // ─── آیا این Trade قبلاً ثبت شده؟ ───
                if (existingByNumber.TryGetValue(dto.TradeNumber, out var existing))
                {
                    // Cancel sync (اگر تازه کنسل شده)
                    if (dto.IsCanceled == true && !existing.IsCanceled && dto.CancelDateTime.HasValue)
                    {
                        existing.MarkCanceled(dto.CancelDateTime.Value);
                        await _positionManager.ApplyTradeCancelAsync(ourOrder, existing, ct);
                        newFilledQty -= existing.Quantity.Value;
                        changed = true;

                        _logger.LogWarning(
                            "Trade canceled: order={OrderId} trade={TradeNumber} qty={Qty}",
                            ourOrder.BrokerOrderId, dto.TradeNumber, existing.Quantity.Value);
                    }

                    continue;
                }

                // ─── Trade جدید ───
                if (dto.IsCanceled == true) continue; // Skip کنسل‌شده‌ها

                var trade = OrderTrade.Create(
                    brokerOrderId: ourOrder.Id,
                    tradeNumber: dto.TradeNumber,
                    isin: ourOrder.Isin,
                    side: ourOrder.Side,
                    price: new Price(dto.Price),
                    quantity: new Quantity(dto.Quantity),
                    tradeDate: dto.TradeDate);

                await _uow.OrderTrades.AddAsync(trade);
                await _positionManager.ApplyTradeAsync(ourOrder, trade, ct);

                newFilledQty += dto.Quantity;
                changed = true;

                _logger.LogInformation(
                    "New trade applied: order={OrderId} trade={TradeNumber} qty={Qty} price={Price} time={Time}",
                    ourOrder.BrokerOrderId, dto.TradeNumber,
                    dto.Quantity, dto.Price, dto.TradeDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to apply trade {TradeNumber} for order {OrderId}.",
                    dto.TradeNumber, ourOrder.BrokerOrderId);
            }
        }

        // ─── بروزرسانی FilledQuantity بر اساس مجموع Tradeهای معتبر ───
        if (newFilledQty != ourOrder.FilledQuantity.Value)
        {
            ourOrder.ApplyStateUpdate(
                ourOrder.State,
                new Quantity(Math.Max(0, newFilledQty)));
            changed = true;
        }

        return changed;
    }
}