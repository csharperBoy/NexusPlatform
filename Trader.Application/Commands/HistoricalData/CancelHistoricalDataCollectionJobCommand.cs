using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;

namespace Trader.Application.Commands.HistoricalData;

public record CancelHistoricalDataCollectionJobCommand(Guid Id)
    : IRequest<Result<bool>>;

public class CancelHistoricalDataCollectionJobCommandHandler
    : IRequestHandler<CancelHistoricalDataCollectionJobCommand, Result<bool>>
{
    private readonly IHistoricalDataCollectionCommandService _service;
    private readonly ILogger<CancelHistoricalDataCollectionJobCommandHandler> _logger;

    public CancelHistoricalDataCollectionJobCommandHandler(
        IHistoricalDataCollectionCommandService service,
        ILogger<CancelHistoricalDataCollectionJobCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CancelHistoricalDataCollectionJobCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cancelled = await _service.CancelAsync(request.Id, cancellationToken);
            if (!cancelled)
                return Result<bool>.Fail(
                    $"Job {request.Id} not found or not cancellable.");

            await _service.SaveAsync(cancellationToken);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel job {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}