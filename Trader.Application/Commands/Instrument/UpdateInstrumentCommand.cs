using Core.Domain.Common;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Instrument;

public record UpdateInstrumentCommand(
    Guid Id,
    Optional<string?> SymbolName,
    Optional<string?> DisplayName,
    Optional<string?> BoardCode,
    Optional<Guid?> TradingScheduleId,
    Optional<decimal> BuyCommissionRate,
    Optional<decimal> SellCommissionRate,
    Optional<long> MinOrderQuantity,
    Optional<long> MaxOrderQuantity,
    Optional<long?> MinOrderValueRial
) : IRequest<Result<bool>>;

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
            var hasChange = await _service.UpdateAsync(
                request.Id,
                request.SymbolName,
                request.DisplayName,
                request.BoardCode,
                request.TradingScheduleId,
                request.BuyCommissionRate,
                request.SellCommissionRate,
                request.MinOrderQuantity,
                request.MaxOrderQuantity,
                request.MinOrderValueRial);

            return Result<bool>.Ok(hasChange);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Instrument {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}