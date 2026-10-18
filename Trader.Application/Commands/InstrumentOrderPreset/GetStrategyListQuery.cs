using Core.Shared.Results;
using MediatR;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Strategies;

namespace Trader.Application.Queries.Strategy;

public record GetStrategyListQuery
    : IRequest<Result<IReadOnlyList<StrategyInfoView>>>;

public class GetStrategyListQueryHandler
    : IRequestHandler<GetStrategyListQuery, Result<IReadOnlyList<StrategyInfoView>>>
{
    private readonly IStrategyQueryService _service;

    public GetStrategyListQueryHandler(IStrategyQueryService service)
        => _service = service;

    public Task<Result<IReadOnlyList<StrategyInfoView>>> Handle(
        GetStrategyListQuery request, CancellationToken cancellationToken)
    {
        var strategies = _service.GetRegisteredStrategies();
        return Task.FromResult(
            Result<IReadOnlyList<StrategyInfoView>>.Ok(strategies));
    }
}