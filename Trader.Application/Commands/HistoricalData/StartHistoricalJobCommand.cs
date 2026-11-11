using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;

namespace Trader.Application.Commands.HistoricalData;

public record StartHistoricalJobCommand(Guid Id) : IRequest<Result<bool>>;

public class StartHistoricalJobCommandHandler
    : IRequestHandler<StartHistoricalJobCommand, Result<bool>>
{
    private readonly IHistoricalDataCollectionCommandService _service;
    private readonly ILogger<StartHistoricalJobCommandHandler> _logger;

    public StartHistoricalJobCommandHandler(
        IHistoricalDataCollectionCommandService service,
        ILogger<StartHistoricalJobCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        StartHistoricalJobCommand request, CancellationToken ct)
    {
        try
        {
            await _service.StartJobAsync(request.Id, ct);
            await _service.SaveAsync(ct);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to start historical job {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}