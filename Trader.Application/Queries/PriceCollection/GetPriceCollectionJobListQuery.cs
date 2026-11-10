using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Queries.PriceCollection;

public record GetPriceCollectionJobListQuery(Guid? PriceSourceId = null, int Take = 100)
    : IRequest<Result<IReadOnlyList<PriceCollectionJobView>>>;

public class GetPriceCollectionJobListQueryHandler
    : IRequestHandler<GetPriceCollectionJobListQuery, Result<IReadOnlyList<PriceCollectionJobView>>>
{
    private readonly IPriceCollectionJobQueryService _service;
    private readonly ILogger<GetPriceCollectionJobListQueryHandler> _logger;

    public GetPriceCollectionJobListQueryHandler(
        IPriceCollectionJobQueryService service,
        ILogger<GetPriceCollectionJobListQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<PriceCollectionJobView>>> Handle(
        GetPriceCollectionJobListQuery request, CancellationToken ct)
    {
        try
        {
            var list = await _service.GetListAsync(request.PriceSourceId, request.Take, ct);
            return Result<IReadOnlyList<PriceCollectionJobView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PriceCollectionJob list");
            return Result<IReadOnlyList<PriceCollectionJobView>>.Fail(ex.Message);
        }
    }
}