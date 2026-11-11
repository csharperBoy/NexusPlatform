using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;

namespace Trader.Application.Commands.HistoricalData;

public record StopHistoricalJobCommand(Guid Id) : IRequest<Result<bool>>;

public class StopHistoricalJobCommandHandler
    : IRequestHandler<StopHistoricalJobCommand, Result<bool>>
{
    private readonly IHistoricalDataCollectionCommandService _service;
    private readonly ILogger<StopHistoricalJobCommandHandler> _logger;

    public StopHistoricalJobCommandHandler(
        IHistoricalDataCollectionCommandService service,
        ILogger<StopHistoricalJobCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        StopHistoricalJobCommand request, CancellationToken ct)
    {
        try
        {
            await _service.StopJobAsync(request.Id, ct);
            await _service.SaveAsync(ct);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to stop historical job {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}