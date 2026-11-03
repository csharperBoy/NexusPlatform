using Microsoft.EntityFrameworkCore;
using Trader.Application.Abstractions.Backtesting;
using Trader.Application.Dtos.Backtests;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public sealed class BacktestEventQueryService : IBacktestEventQueryService
{
    private readonly TraderDbContext _db;

    public BacktestEventQueryService(TraderDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<BacktestEventView> Items, int TotalCount)> QueryAsync(
        BacktestEventQuery query, CancellationToken ct = default)
    {
        var q = _db.BacktestEvents
            .AsNoTracking()
            .Where(e => e.BacktestRunId == query.BacktestRunId);

        if (query.Type.HasValue)
            q = q.Where(e => (int)e.Type == query.Type.Value);

        if (query.MinSeverity.HasValue)
            q = q.Where(e => (int)e.Severity >= query.MinSeverity.Value);

        if (!string.IsNullOrWhiteSpace(query.Isin))
            q = q.Where(e => e.Isin == query.Isin);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(e => e.Message.Contains(s));
        }

        var total = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .CountAsync(q, ct);

        var items = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(
                q.OrderBy(e => e.OccurredAt)
                 .Skip(Math.Max(0, query.Skip))
                 .Take(Math.Clamp(query.Take, 1, 5000)),
                ct);

        return (items.Select(MapToView).ToList(), total);
    }

    private static BacktestEventView MapToView(BacktestEvent e) => new(
        e.Id,
        e.BacktestRunId,
        (int)e.Type,
        e.Type.ToString(),
        (int)e.Severity,
        e.Severity.ToString(),
        e.OccurredAt,
        e.Message,
        e.PayloadJson,
        e.Isin,
        e.CandleTime,
        e.TradeId);
}