using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Specifications.OrderIntents;
using Trader.Application.Specifications.Positions;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Execution;

public sealed class CommittedCapitalCalculator : ICommittedCapitalCalculator
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;

    public CommittedCapitalCalculator(ITraderUnitOfWork<TraderDbContext> uow)
    {
        _uow = uow;
    }

    public async Task<decimal> CalculateAsync(
        Guid runId, Guid accountId, CancellationToken ct = default)
    {
        // ─── ۱. OrderIntentهای فعال (سفارشات در راه) ───
        var activeIntents = (await _uow.OrderIntentSpecs.ListBySpecAsync(
            new PendingOrderIntentsByRunSpec(runId)))
            .Where(i => i.TraderAccountId == accountId);

        decimal intentsTotal = 0m;
        foreach (var i in activeIntents)
        {
            intentsTotal += i.Price.Value * i.Quantity.Value;
        }

        // ─── ۲. Positionهای باز (سهام موجود) ───
        var openPositions = (await _uow.PositionSpecs.ListBySpecAsync(
            new OpenPositionsByAccountSpec(accountId)))
            .Where(p => p.RunId == runId && p.State != PositionState.Closed);

        decimal positionsTotal = 0m;
        foreach (var p in openPositions)
        {
            positionsTotal += p.AverageEntryPrice.Value * p.Quantity.Value;
        }

        return intentsTotal + positionsTotal;
    }
}