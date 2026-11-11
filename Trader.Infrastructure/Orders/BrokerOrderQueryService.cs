using Microsoft.EntityFrameworkCore;
using Trader.Application.Abstractions.Orders;
using Trader.Application.Dtos.Orders;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.Orders;

namespace Trader.Infrastructure.Orders;

public sealed class BrokerOrderQueryService : IBrokerOrderQueryService
{
    private readonly TraderDbContext _db;

    public BrokerOrderQueryService(TraderDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<BrokerOrderView> Items, int TotalCount)> QueryAsync(
        BrokerOrderQuery query, CancellationToken ct = default)
    {
        var q = _db.BrokerOrders.AsNoTracking().AsQueryable();

        if (query.RunId.HasValue)
            q = q.Where(o => o.RunId == query.RunId.Value);

        if (query.AccountId.HasValue)
            q = q.Where(o => o.TraderAccountId == query.AccountId.Value);

        if (!string.IsNullOrWhiteSpace(query.Isin))
            q = q.Where(o => o.Isin == query.Isin.Trim());

        if (query.State.HasValue)
            q = q.Where(o => (int)o.State == query.State.Value);

        if (query.IsPaper.HasValue)
            q = q.Where(o => o.IsPaper == query.IsPaper.Value);

        if (query.SubmittedFrom.HasValue)
            q = q.Where(o => o.SubmittedAt >= query.SubmittedFrom.Value);

        if (query.SubmittedTo.HasValue)
            q = q.Where(o => o.SubmittedAt <= query.SubmittedTo.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(o =>
                o.Isin.Contains(s)
                || (o.BrokerOrderId != null && o.BrokerOrderId.Contains(s)));
        }

        var total = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(o => o.SubmittedAt)
            .Skip(Math.Max(0, query.Skip))
            .Take(Math.Clamp(query.Take, 1, 500))
            .ToListAsync(ct);

        var views = await MapManyAsync(rows, ct);
        return (views, total);
    }

    public async Task<BrokerOrderDetailView?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var order = await _db.BrokerOrders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return null;

        var views = await MapManyAsync(new[] { order }, ct);
        var summary = views[0];

        var trades = await _db.OrderTrades.AsNoTracking()
            .Where(t => t.BrokerOrderId == id)
            .OrderBy(t => t.TradeDate)
            .Select(t => new OrderTradeView(
                t.Id,
                t.TradeNumber,
                t.Price.Value,
                t.Quantity.Value,
                t.TradeDate,
                t.IsCanceled,
                t.CancelDateTime,
                t.CreatedAt))
            .ToListAsync(ct);

        return new BrokerOrderDetailView(summary, trades);
    }

    /* ═══════════ Mappers ═══════════ */

    private async Task<IReadOnlyList<BrokerOrderView>> MapManyAsync(
        IReadOnlyList<BrokerOrder> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<BrokerOrderView>();

        var accountIds = rows.Select(r => r.TraderAccountId).Distinct().ToList();
        var accounts = await _db.Accounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, ct);

        var isins = rows.Select(r => r.Isin).Distinct().ToList();
        var symbols = await _db.Instruments.AsNoTracking()
            .Where(i => isins.Contains(i.Isin))
            .ToDictionaryAsync(i => i.Isin, i => i.SymbolName, ct);

        return rows.Select(o =>
        {
            accounts.TryGetValue(o.TraderAccountId, out var accountName);
            symbols.TryGetValue(o.Isin, out var symbolName);

            return new BrokerOrderView(
                Id: o.Id,
                OrderIntentId: o.OrderIntentId,
                RunId: o.RunId,
                TraderAccountId: o.TraderAccountId,
                AccountName: accountName,
                Isin: o.Isin,
                SymbolName: symbolName,
                Side: (int)o.Side,
                SideName: OrderLabels.Side(o.Side),
                Price: o.Price.Value,
                Quantity: o.Quantity.Value,
                FilledQuantity: o.FilledQuantity.Value,
                FilledRatio: Math.Round(o.FilledRatio * 100m, 2),
                State: (int)o.State,
                StateName: OrderLabels.State(o.State),
                IsTerminal: o.IsTerminal,
                IsPaper: o.IsPaper,
                BrokerOrderId: o.BrokerOrderId,
                SubmittedAt: o.SubmittedAt,
                LastSyncedAt: o.LastSyncedAt,
                CreatedAt: o.CreatedAt);
        }).ToList();
    }
}