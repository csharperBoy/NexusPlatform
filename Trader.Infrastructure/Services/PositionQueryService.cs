using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Positions;
using Trader.Application.Specifications.Positions;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public class PositionQueryService : IPositionQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<PositionQueryService> _logger;

    public PositionQueryService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<PositionQueryService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PositionView>> GetOpenByRunAsync(
        Guid runId, CancellationToken ct = default)
    {
        var spec = new OpenPositionsByRunSpec(runId);
        var positions = await _uow.PositionSpecs.ListBySpecAsync(spec);
        return positions.Select(MapToView).ToList();
    }

    public async Task<IReadOnlyList<PositionView>> GetOpenByAccountAsync(
        Guid accountId, CancellationToken ct = default)
    {
        var spec = new OpenPositionsByAccountSpec(accountId);
        var positions = await _uow.PositionSpecs.ListBySpecAsync(spec);
        return positions.Select(MapToView).ToList();
    }

    private static PositionView MapToView(Position p) => new(
        p.Id,
        p.RunId,
        p.TraderAccountId,
        p.Isin,
        (int)p.State,
        p.State.ToString(),
        p.Quantity.Value,
        p.AverageEntryPrice.Value,
        p.RealizedPnL.Amount,
        p.UnrealizedPnL?.Amount,
        p.StopLossPrice?.Value,
        p.TakeProfitPrice?.Value,
        p.OpenedAt,
        p.ClosedAt);
}