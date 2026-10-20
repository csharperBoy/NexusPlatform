using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.Instruments;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Commands.Instrument;

public record CreateInstrumentCommand(
    string Isin,
    string SymbolName,
    string DisplayName,
    int Type,
    int Market,
    string? BoardCode,
    OptionContractModel? OptionContract
) : IRequest<Result<Guid>>;

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
            var model = new CreateInstrumentModel(
                request.Isin,
                request.SymbolName,
                request.DisplayName,
                request.Type,
                request.Market,
                request.BoardCode,
                request.OptionContract);

            var id = await _service.CreateAsync(model);
            _logger.LogInformation("Instrument created: {Id} isin={Isin}", id, request.Isin);
            return Result<Guid>.Ok(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create Instrument {Isin}", request.Isin);
            return Result<Guid>.Fail(ex.Message);
        }
    }
}