using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;

namespace Trader.Application.Commands.PriceCollection;

public record StopPriceCollectionJobCommand(Guid Id) : IRequest<Result<bool>>;

public class StopPriceCollectionJobCommandHandler
    : IRequestHandler<StopPriceCollectionJobCommand, Result<bool>>
{
    private readonly IPriceCollectionService _service;
    private readonly ILogger<StopPriceCollectionJobCommandHandler> _logger;

    public StopPriceCollectionJobCommandHandler(
        IPriceCollectionService service,
        ILogger<StopPriceCollectionJobCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        StopPriceCollectionJobCommand request, CancellationToken ct)
    {
        try
        {
            await _service.StopJobAsync(request.Id);
            await _service.SaveAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop PriceCollectionJob {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}