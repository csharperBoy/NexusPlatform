using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Dtos.HistoricalData;

namespace Trader.Application.Queries.HistoricalData;

public record GetHistoricalDataCollectionJobByIdQuery(Guid Id)
    : IRequest<Result<HistoricalDataCollectionJobDetailView>>;

public class GetHistoricalDataCollectionJobByIdQueryHandler
    : IRequestHandler<GetHistoricalDataCollectionJobByIdQuery,
        Result<HistoricalDataCollectionJobDetailView>>
{
    private readonly IHistoricalDataCollectionQueryService _service;
    private readonly ILogger<GetHistoricalDataCollectionJobByIdQueryHandler> _logger;

    public GetHistoricalDataCollectionJobByIdQueryHandler(
        IHistoricalDataCollectionQueryService service,
        ILogger<GetHistoricalDataCollectionJobByIdQueryHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<HistoricalDataCollectionJobDetailView>> Handle(
        GetHistoricalDataCollectionJobByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var detail = await _service.GetByIdAsync(request.Id, cancellationToken);
            if (detail is null)
                return Result<HistoricalDataCollectionJobDetailView>.Fail(
                    $"Job {request.Id} not found.");

            return Result<HistoricalDataCollectionJobDetailView>.Ok(detail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get job {Id}", request.Id);
            return Result<HistoricalDataCollectionJobDetailView>.Fail(ex.Message);
        }
    }
}