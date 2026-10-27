using Microsoft.EntityFrameworkCore;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.TradingEvents;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public sealed class TradingEventQueryService : ITradingEventQueryService
{
    private readonly TraderDbContext _db;

    public TradingEventQueryService(TraderDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<TradingEventView> Items, int TotalCount)> QueryAsync(
        TradingEventQuery query, CancellationToken ct = default)
    {
        var q = _db.TradingEvents.AsNoTracking();

        if (query.RunId.HasValue)
            q = q.Where(e => e.RunId == query.RunId.Value);

        if (!string.IsNullOrWhiteSpace(query.Isin))
            q = q.Where(e => e.Isin == query.Isin);

        if (query.AccountId.HasValue)
            q = q.Where(e => e.AccountId == query.AccountId.Value);

        if (query.Stage.HasValue)
            q = q.Where(e => (int)e.Stage == query.Stage.Value);

        if (query.Severity.HasValue)
            q = q.Where(e => (int)e.Severity == query.Severity.Value);

        if (query.MinSeverity.HasValue)
            q = q.Where(e => (int)e.Severity >= query.MinSeverity.Value);

        if (query.From.HasValue)
            q = q.Where(e => e.OccurredAt >= query.From.Value);

        if (query.To.HasValue)
            q = q.Where(e => e.OccurredAt <= query.To.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(e => EF.Functions.Like(e.Message, $"%{s}%"));
        }

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(e => e.OccurredAt)
            .Skip(query.Skip)
            .Take(Math.Clamp(query.Take, 1, 1000))
            .Select(e => MapToView(e))
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<TradingEventView?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
    {
        var e = await _db.TradingEvents.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return e is null ? null : MapToView(e);
    }

    /* ═══════════════════ Mapper ═══════════════════ */

    private static TradingEventView MapToView(TradingEvent e) => new(
        e.Id,
        e.RunId,
        e.Isin,
        e.AccountId,
        e.SignalId,
        e.OrderIntentId,
        e.BrokerOrderId,
        e.TradeId,
        e.PositionId,
        (int)e.Stage,
        e.Stage.ToString(),
        (int)e.Type,
        e.Type.ToString(),
        (int)e.Severity,
        e.Severity.ToString(),
        e.OccurredAt,
        e.Message,
        e.PayloadJson);
}