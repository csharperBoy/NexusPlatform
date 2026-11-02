using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;

namespace Trader.Application.Commands.HistoricalData;

public record CollectHistoricalDataCommand(
    string Name,
    List<string> Isins,
    List<int> TimeFrames,
    DateTime DateFrom,
    DateTime DateTo
) : IRequest<Result<Guid>>;

public class CollectHistoricalDataCommandHandler
    : IRequestHandler<CollectHistoricalDataCommand, Result<Guid>>
{
    private readonly IHistoricalDataCollectionCommandService _service;
    private readonly ILogger<CollectHistoricalDataCommandHandler> _logger;

    public CollectHistoricalDataCommandHandler(
        IHistoricalDataCollectionCommandService service,
        ILogger<CollectHistoricalDataCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CollectHistoricalDataCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating collection job: {Name} isins={IsinCount} tfs={TfCount}",
                request.Name,
                request.Isins?.Count ?? 0,
                request.TimeFrames?.Count ?? 0);

            var id = await _service.CreateAsync(
                request.Name,
                request.Isins ?? new(),
                request.TimeFrames ?? new(),
                request.DateFrom,
                request.DateTo,
                cancellationToken);

            await _service.SaveAsync(cancellationToken);

            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create collection job {Name}", request.Name);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}