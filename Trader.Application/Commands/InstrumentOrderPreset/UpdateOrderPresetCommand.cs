using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Commands.InstrumentOrderPreset;

public record UpdateOrderPresetCommand(
    Guid Id,
    UpdateOrderPresetModel Model
) : IRequest<Result<bool>>;

public class UpdateOrderPresetCommandHandler
    : IRequestHandler<UpdateOrderPresetCommand, Result<bool>>
{
    private readonly IInstrumentOrderPresetService _service;
    private readonly ILogger<UpdateOrderPresetCommandHandler> _logger;

    public UpdateOrderPresetCommandHandler(
        IInstrumentOrderPresetService service,
        ILogger<UpdateOrderPresetCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateOrderPresetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateAsync(request.Id, request.Model);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update OrderPreset {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}