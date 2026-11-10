using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;

namespace Trader.Application.Commands.PriceCollection;

public record DeletePriceSourceCommand(Guid Id) : IRequest<Result<bool>>;

public class DeletePriceSourceCommandHandler
    : IRequestHandler<DeletePriceSourceCommand, Result<bool>>
{
    private readonly IPriceCollectionService _service;
    private readonly ILogger<DeletePriceSourceCommandHandler> _logger;

    public DeletePriceSourceCommandHandler(
        IPriceCollectionService service,
        ILogger<DeletePriceSourceCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeletePriceSourceCommand request, CancellationToken ct)
    {
        try
        {
            await _service.DeactivateSourceAsync(request.Id);
            await _service.SaveAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate PriceSource {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}