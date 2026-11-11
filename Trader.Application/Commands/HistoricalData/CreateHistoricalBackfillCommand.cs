using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Dtos.HistoricalData;

namespace Trader.Application.Commands.HistoricalData;

public record CreateHistoricalBackfillCommand(
    string Name,
    List<string> Isins,
    List<int> TimeFrames,
    DateTime DateFrom,
    DateTime DateTo) : IRequest<Result<Guid>>;

public class CreateHistoricalBackfillCommandHandler
    : IRequestHandler<CreateHistoricalBackfillCommand, Result<Guid>>
{
    private readonly IHistoricalDataCollectionCommandService _service;
    private readonly ILogger<CreateHistoricalBackfillCommandHandler> _logger;

    public CreateHistoricalBackfillCommandHandler(
        IHistoricalDataCollectionCommandService service,
        ILogger<CreateHistoricalBackfillCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateHistoricalBackfillCommand request, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Creating backfill job: name={Name} isins={I} tfs={T} range={From}→{To}",
                request.Name, request.Isins?.Count ?? 0, request.TimeFrames?.Count ?? 0,
                request.DateFrom, request.DateTo);

            var model = new CreateHistoricalBackfillModel(
                request.Name,
                request.Isins ?? new(),
                request.TimeFrames ?? new(),
                request.DateFrom,
                request.DateTo);

            var id = await _service.CreateBackfillAsync(model, ct);
            await _service.SaveAsync(ct);

            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create backfill job {Name}", request.Name);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}