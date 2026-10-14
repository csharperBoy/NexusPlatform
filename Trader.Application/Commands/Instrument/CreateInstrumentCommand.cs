using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;

namespace Trader.Application.Commands.Instrument;

public record CreateInstrumentCommand(CreateInstrumentModel Model)
    : IRequest<Result<Guid>>;

public class CreateInstrumentCommandHandler
    : IRequestHandler<CreateInstrumentCommand, Result<Guid>>
{
    private readonly IInstrumentCommandService _service;
    private readonly ILogger<CreateInstrumentCommandHandler> _logger;

    public CreateInstrumentCommandHandler(
        IInstrumentCommandService service,
        ILogger<CreateInstrumentCommandHandler> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateInstrumentCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _service.CreateAsync(request.Model);
            _logger.LogInformation("Created Instrument {Id}", id);
            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create Instrument {Isin}", request.Model.Isin);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}