using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Dtos.PriceCollection;

namespace Trader.Application.Commands.PriceCollection;

public record CreatePriceSourceCommand(
    string Code,
    string Name,
    int Type,
    string ConfigJson,
    string FromAsset,
    string ToAsset) : IRequest<Result<Guid>>;

public class CreatePriceSourceCommandHandler
    : IRequestHandler<CreatePriceSourceCommand, Result<Guid>>
{
    private readonly IPriceCollectionService _service;
    private readonly ILogger<CreatePriceSourceCommandHandler> _logger;

    public CreatePriceSourceCommandHandler(
        IPriceCollectionService service,
        ILogger<CreatePriceSourceCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreatePriceSourceCommand request, CancellationToken ct)
    {
        try
        {
            var model = new CreatePriceSourceModel(
                request.Code, request.Name, request.Type,
                request.ConfigJson, request.FromAsset, request.ToAsset);

            var id = await _service.CreateSourceAsync(model);
            await _service.SaveAsync();

            _logger.LogInformation("PriceSource created: {Id} code={Code}", id, request.Code);
            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PriceSource {Code}", request.Code);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}