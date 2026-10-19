using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Positions;

namespace Trader.Application.Queries.Position;

public record GetRunPositionsQuery(Guid RunId)
    : IRequest<Result<IReadOnlyList<PositionView>>>;

public class GetRunPositionsQueryHandler
    : IRequestHandler<GetRunPositionsQuery, Result<IReadOnlyList<PositionView>>>
{
    private readonly IPositionQueryService _service;
    private readonly ILogger<GetRunPositionsQueryHandler> _logger;

    public GetRunPositionsQueryHandler(
        IPositionQueryService service,
        ILogger<GetRunPositionsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<PositionView>>> Handle(
        GetRunPositionsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var positions = await _service.GetOpenByRunAsync(
                request.RunId, cancellationToken);
            return Result<IReadOnlyList<PositionView>>.Ok(positions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get positions for run {RunId}", request.RunId);
            return Result<IReadOnlyList<PositionView>>.Fail(ex.Message);
        }
    }
}