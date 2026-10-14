using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Positions;

namespace Trader.Application.Queries.Position;

public record GetAccountPositionsQuery(Guid AccountId)
    : IRequest<Result<IReadOnlyList<PositionView>>>;

public class GetAccountPositionsQueryHandler
    : IRequestHandler<GetAccountPositionsQuery, Result<IReadOnlyList<PositionView>>>
{
    private readonly IPositionQueryService _service;
    private readonly ILogger<GetAccountPositionsQueryHandler> _logger;

    public GetAccountPositionsQueryHandler(
        IPositionQueryService service,
        ILogger<GetAccountPositionsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<PositionView>>> Handle(
        GetAccountPositionsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var positions = await _service.GetOpenByAccountAsync(
                request.AccountId, cancellationToken);
            return Result<IReadOnlyList<PositionView>>.Ok(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get positions for account {AccountId}", request.AccountId);
            return Result<IReadOnlyList<PositionView>>.Fail(ex.Message);
        }
    }
}