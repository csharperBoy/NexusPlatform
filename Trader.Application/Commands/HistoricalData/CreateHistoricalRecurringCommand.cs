using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Dtos.HistoricalData;

namespace Trader.Application.Commands.HistoricalData;

public record CreateHistoricalRecurringCommand(
    string Name,
    List<string> Isins,
    List<int> TimeFrames,
    DateTime DateFrom,
    string RecurrencePatternJson) : IRequest<Result<Guid>>;

public class CreateHistoricalRecurringCommandHandler
    : IRequestHandler<CreateHistoricalRecurringCommand, Result<Guid>>
{
    private readonly IHistoricalDataCollectionCommandService _service;
    private readonly ILogger<CreateHistoricalRecurringCommandHandler> _logger;

    public CreateHistoricalRecurringCommandHandler(
        IHistoricalDataCollectionCommandService service,
        ILogger<CreateHistoricalRecurringCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateHistoricalRecurringCommand request, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Creating recurring job: name={Name} isins={I} tfs={T}",
                request.Name, request.Isins?.Count ?? 0, request.TimeFrames?.Count ?? 0);

            var model = new CreateHistoricalRecurringModel(
                request.Name,
                request.Isins ?? new(),
                request.TimeFrames ?? new(),
                request.DateFrom,
                request.RecurrencePatternJson);

            var id = await _service.CreateRecurringAsync(model, ct);
            await _service.SaveAsync(ct);

            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create recurring job {Name}", request.Name);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}