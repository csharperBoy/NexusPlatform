using Core.Application.Results;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Symbol
{
    public record UpdateSymbolCommand(
        Guid Id,
        string SymbolName,
        string SymbolIsin,
        long Price,
        long Quantity,
        int Side,
        int ValidityType,
        decimal Commission,
        int OrderModelType,
        int OrderFrom
    ) : IRequest<Result<Guid>>;

    public class UpdateSymbolCommandHandler
        : IRequestHandler<UpdateSymbolCommand, Result<Guid>>
    {
        private readonly ISymbolCommandService _symbolService;
        private readonly ILogger<UpdateSymbolCommandHandler> _logger;

        public UpdateSymbolCommandHandler(
            ISymbolCommandService symbolService,
            ILogger<UpdateSymbolCommandHandler> logger)
        {
            _symbolService = symbolService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            UpdateSymbolCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Updating TraderSymbol: {Id}", request.Id);

                var id = await _symbolService.UpdateSymbolAsync(
                    request.Id,
                    request.SymbolName,
                    request.SymbolIsin,
                    request.Price,
                    request.Quantity,
                    request.Side,
                    request.ValidityType,
                    request.Commission,
                    request.OrderModelType,
                    request.OrderFrom);

                await _symbolService.SaveAsync();

                _logger.LogInformation(
                    "TraderSymbol updated: {Id}", id);

                return Result<Guid>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update TraderSymbol: {Id}", request.Id);
                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}