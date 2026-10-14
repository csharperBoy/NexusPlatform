using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Instrument;

public record DeactivateInstrumentCommand(Guid Id) : IRequest<Result<bool>>;

public class DeactivateInstrumentCommandHandler
    : IRequestHandler<DeactivateInstrumentCommand, Result<bool>>
{
    private readonly IInstrumentCommandService _service;
    private readonly ILogger<DeactivateInstrumentCommandHandler> _logger;

    public DeactivateInstrumentCommandHandler(
        IInstrumentCommandService service,
        ILogger<DeactivateInstrumentCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DeactivateInstrumentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeactivateAsync(request.Id);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate Instrument {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}