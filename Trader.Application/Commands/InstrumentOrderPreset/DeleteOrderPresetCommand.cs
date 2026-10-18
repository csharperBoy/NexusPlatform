using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.InstrumentOrderPreset;

public record DeleteOrderPresetCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteOrderPresetCommandHandler
    : IRequestHandler<DeleteOrderPresetCommand, Result<bool>>
{
    private readonly IInstrumentOrderPresetService _service;
    private readonly ILogger<DeleteOrderPresetCommandHandler> _logger;

    public DeleteOrderPresetCommandHandler(
        IInstrumentOrderPresetService service,
        ILogger<DeleteOrderPresetCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeleteOrderPresetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(request.Id);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete OrderPreset {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}