using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Dtos.HistoricalData;

namespace Trader.Application.Queries.HistoricalData;

public record GetHistoricalDataCollectionJobsQuery(int Take = 50)
    : IRequest<Result<IReadOnlyList<HistoricalDataCollectionJobSummaryView>>>;

public class GetHistoricalDataCollectionJobsQueryHandler
    : IRequestHandler<GetHistoricalDataCollectionJobsQuery,
        Result<IReadOnlyList<HistoricalDataCollectionJobSummaryView>>>
{
    private readonly IHistoricalDataCollectionQueryService _service;
    private readonly ILogger<GetHistoricalDataCollectionJobsQueryHandler> _logger;

    public GetHistoricalDataCollectionJobsQueryHandler(
        IHistoricalDataCollectionQueryService service,
        ILogger<GetHistoricalDataCollectionJobsQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<HistoricalDataCollectionJobSummaryView>>> Handle(
        GetHistoricalDataCollectionJobsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetListAsync(request.Take, cancellationToken);
            return Result<IReadOnlyList<HistoricalDataCollectionJobSummaryView>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get collection jobs");
            return Result<IReadOnlyList<HistoricalDataCollectionJobSummaryView>>.Fail(ex.Message);
        }
    }
}