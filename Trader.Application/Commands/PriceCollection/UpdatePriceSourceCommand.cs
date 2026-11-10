using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Commands.PriceCollection;

public record UpdatePriceSourceCommand(
    Guid Id,
    string Name,
    string ConfigJson,
    string FromAsset,
    string ToAsset) : IRequest<Result<bool>>;

public class UpdatePriceSourceCommandHandler
    : IRequestHandler<UpdatePriceSourceCommand, Result<bool>>
{
    private readonly IPriceCollectionService _service;
    private readonly ILogger<UpdatePriceSourceCommandHandler> _logger;

    public UpdatePriceSourceCommandHandler(
        IPriceCollectionService service,
        ILogger<UpdatePriceSourceCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdatePriceSourceCommand request, CancellationToken ct)
    {
        try
        {
            var model = new CreatePriceSourceModel(
                Code: "", // ignored in update
                request.Name, Type: 0, // ignored
                request.ConfigJson, request.FromAsset, request.ToAsset);

            await _service.UpdateSourceAsync(request.Id, model);
            await _service.SaveAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update PriceSource {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}