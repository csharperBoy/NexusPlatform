using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Signals;
using Trader.Application.Specifications.Signals;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services;

public class SignalQueryService : ISignalQueryService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly ILogger<SignalQueryService> _logger;

    public SignalQueryService(
        ITraderUnitOfWork<TraderDbContext> uow,
        ILogger<SignalQueryService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SignalView>> GetRecentByRunAsync(
        Guid runId, int take = 100, CancellationToken ct = default)
    {
        var spec = new RecentSignalsByRunSpec(runId, take);
        var signals = await _uow.SignalSpecs.ListBySpecAsync(spec);

        return signals.Select(MapToView).ToList();
    }

    private static SignalView MapToView(Signal s) => new(
        s.Id,
        s.RunId,
        s.Isin,
        (int)s.Action,
        s.Action.ToString(),
        s.Strength.Value,
        s.StrategyKey,
        s.ProducedAt,
        s.ConsumedAt);
}