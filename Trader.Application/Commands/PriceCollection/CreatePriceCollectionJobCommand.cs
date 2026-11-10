using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Commands.PriceCollection;

public record CreatePriceCollectionJobCommand(
    Guid PriceSourceId,
    int Mode,
    string? RecurrencePatternJson) : IRequest<Result<Guid>>;

public class CreatePriceCollectionJobCommandHandler
    : IRequestHandler<CreatePriceCollectionJobCommand, Result<Guid>>
{
    private readonly IPriceCollectionService _service;
    private readonly ILogger<CreatePriceCollectionJobCommandHandler> _logger;

    public CreatePriceCollectionJobCommandHandler(
        IPriceCollectionService service,
        ILogger<CreatePriceCollectionJobCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreatePriceCollectionJobCommand request, CancellationToken ct)
    {
        try
        {
            var model = new CreatePriceCollectionJobModel(
                request.PriceSourceId, request.Mode, request.RecurrencePatternJson);

            var id = await _service.CreateJobAsync(model);
            await _service.SaveAsync();
            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PriceCollectionJob");
            return Result<Guid>.Fail(ex.Message);
        }
    }
}