using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Commands.InstrumentOrderPreset;

public record CreateOrderPresetCommand(
    CreateOrderPresetModel Model
) : IRequest<Result<Guid>>;

public class CreateOrderPresetCommandHandler
    : IRequestHandler<CreateOrderPresetCommand, Result<Guid>>
{
    private readonly IInstrumentOrderPresetService _service;
    private readonly ILogger<CreateOrderPresetCommandHandler> _logger;

    public CreateOrderPresetCommandHandler(
        IInstrumentOrderPresetService service,
        ILogger<CreateOrderPresetCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrderPresetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _service.CreateAsync(request.Model);
            _logger.LogInformation("Created OrderPreset {Id}", id);
            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create OrderPreset");
            return Result<Guid>.Fail(ex.Message);
        }
    }
}