using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Specifications.BrokerOrders;
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

    public Task<decimal> CalculateForRunAsync(
        Guid runId, Guid accountId, CancellationToken ct = default)
        => CalculateInternalAsync(runId, accountId, isin: null, ct);

    public Task<decimal> CalculateForSymbolAsync(
        Guid runId, Guid accountId, string isin, CancellationToken ct = default)
        => CalculateInternalAsync(runId, accountId, isin, ct);

    private async Task<decimal> CalculateInternalAsync(
        Guid runId, Guid accountId, string? isin, CancellationToken ct)
    {
        // ─── ۱. Positionهای باز (سرمایه‌ی از دست رفته در خرید) ───
        var positions = (await _uow.PositionSpecs.ListBySpecAsync(
            new OpenPositionsByRunSpec(runId)))
            .Where(p => p.TraderAccountId == accountId
                     && p.State != PositionState.Closed
                     && (isin == null || p.Isin == isin));

        decimal positionsValue = 0m;
        foreach (var p in positions)
            positionsValue += p.AverageEntryPrice.Value * p.Quantity.Value;

        // ─── ۲. سفارشات BUY در راه (سرمایه‌ی هنوز خرج نشده، ولی رزرو شده) ───
        var activeOrders = (await _uow.BrokerOrderSpecs.ListBySpecAsync(
            new OpenBrokerOrdersByAccountSpec(accountId)))
            .Where(o => o.RunId == runId
                     && !o.IsTerminal
                     && o.Side == OrderSide.Buy
                     && (isin == null || o.Isin == isin));

        decimal activeOrdersValue = 0m;
        foreach (var o in activeOrders)
        {
            var remaining = o.Quantity.Value - o.FilledQuantity.Value;
            if (remaining > 0)
                activeOrdersValue += o.Price.Value * remaining;
        }

        return positionsValue + activeOrdersValue;
    }
}