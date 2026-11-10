using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;

namespace Trader.Application.Commands.PriceCollection;

public record StartPriceCollectionJobCommand(Guid Id) : IRequest<Result<bool>>;

public class StartPriceCollectionJobCommandHandler
    : IRequestHandler<StartPriceCollectionJobCommand, Result<bool>>
{
    private readonly IPriceCollectionService _service;
    private readonly ILogger<StartPriceCollectionJobCommandHandler> _logger;

    public StartPriceCollectionJobCommandHandler(
        IPriceCollectionService service,
        ILogger<StartPriceCollectionJobCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        StartPriceCollectionJobCommand request, CancellationToken ct)
    {
        try
        {
            await _service.StartJobAsync(request.Id);
            await _service.SaveAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start PriceCollectionJob {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}