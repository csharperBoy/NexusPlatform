using Microsoft.EntityFrameworkCore;
using Trader.Application.Abstractions.Orders;
using Trader.Application.Dtos.Orders;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.Orders;

namespace Trader.Infrastructure.Orders;

public sealed class OrderIntentQueryService : IOrderIntentQueryService
{
    private readonly TraderDbContext _db;

    public OrderIntentQueryService(TraderDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<OrderIntentView> Items, int TotalCount)> QueryAsync(
        OrderIntentQuery query, CancellationToken ct = default)
    {
        var q = _db.OrderIntents.AsNoTracking().AsQueryable();

        if (query.RunId.HasValue)
            q = q.Where(i => i.RunId == query.RunId.Value);

        if (query.AccountId.HasValue)
            q = q.Where(i => i.TraderAccountId == query.AccountId.Value);

        if (!string.IsNullOrWhiteSpace(query.Isin))
            q = q.Where(i => i.Isin == query.Isin.Trim());

        if (query.State.HasValue)
            q = q.Where(i => (int)i.State == query.State.Value);

        if (query.IsPaper.HasValue)
            q = q.Where(i => i.IsPaper == query.IsPaper.Value);

        if (query.From.HasValue)
            q = q.Where(i => i.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(i => i.CreatedAt <= query.To.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(i =>
                i.Isin.Contains(s)
                || (i.FailureReason != null && i.FailureReason.Contains(s))
                || (i.Reason != null && i.Reason.Contains(s)));
        }

        var total = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(i => i.CreatedAt)
            .Skip(Math.Max(0, query.Skip))
            .Take(Math.Clamp(query.Take, 1, 500))
            .ToListAsync(ct);

        var views = await MapManyAsync(rows, ct);
        return (views, total);
    }

    public async Task<OrderIntentView?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var intent = await _db.OrderIntents.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);
        if (intent is null) return null;

        var views = await MapManyAsync(new[] { intent }, ct);
        return views[0];
    }

    /* ═══════════ Mappers ═══════════ */

    private async Task<IReadOnlyList<OrderIntentView>> MapManyAsync(
        IReadOnlyList<OrderIntent> rows, CancellationToken ct)
    {
        if (rows.Count == 0) return Array.Empty<OrderIntentView>();

        var accountIds = rows.Select(r => r.TraderAccountId).Distinct().ToList();
        var accounts = await _db.Accounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, ct);

        var isins = rows.Select(r => r.Isin).Distinct().ToList();
        var symbols = await _db.Instruments.AsNoTracking()
            .Where(i => isins.Contains(i.Isin))
            .ToDictionaryAsync(i => i.Isin, i => i.SymbolName, ct);

        return rows.Select(i =>
        {
            accounts.TryGetValue(i.TraderAccountId, out var accountName);
            symbols.TryGetValue(i.Isin, out var symbolName);

            return new OrderIntentView(
                Id: i.Id,
                RunId: i.RunId,
                TraderAccountId: i.TraderAccountId,
                AccountName: accountName,
                SourceSignalId: i.SourceSignalId,
                SourceEvaluationId: i.SourceEvaluationId,
                Isin: i.Isin,
                SymbolName: symbolName,
                Side: (int)i.Side,
                SideName: OrderLabels.Side(i.Side),
                Price: i.Price.Value,
                Quantity: i.Quantity.Value,
                OrderMode: (int)i.OrderMode,
                OrderModeName: OrderLabels.Mode(i.OrderMode),
                State: (int)i.State,
                StateName: OrderLabels.IntentState(i.State),
                Reason: i.Reason,
                FailureReason: i.FailureReason,
                StopLossPrice: i.StopLossPrice?.Value,
                TakeProfitPrice: i.TakeProfitPrice?.Value,
                IsPaper: i.IsPaper,
                DispatchedAt: i.DispatchedAt,
                CompletedAt: i.CompletedAt,
                CreatedAt: i.CreatedAt);
        }).ToList();
    }
}