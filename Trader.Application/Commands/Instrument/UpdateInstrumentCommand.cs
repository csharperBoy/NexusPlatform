using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Commands.Instrument;

public record UpdateInstrumentCommand(Guid Id, UpdateInstrumentModel Model)
    : IRequest<Result<bool>>;

public class UpdateInstrumentCommandHandler
    : IRequestHandler<UpdateInstrumentCommand, Result<bool>>
{
    private readonly IInstrumentCommandService _service;
    private readonly ILogger<UpdateInstrumentCommandHandler> _logger;

    public UpdateInstrumentCommandHandler(
        IInstrumentCommandService service,
        ILogger<UpdateInstrumentCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        UpdateInstrumentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.UpdateAsync(request.Id, request.Model);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Instrument {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}