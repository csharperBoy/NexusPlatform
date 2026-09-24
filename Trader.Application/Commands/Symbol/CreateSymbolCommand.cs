 
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Symbol
{
    public record CreateSymbolCommand(
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

    public class CreateSymbolCommandHandler
        : IRequestHandler<CreateSymbolCommand, Result<Guid>>
    {
        private readonly ISymbolCommandService _symbolService;
        private readonly ILogger<CreateSymbolCommandHandler> _logger;

        public CreateSymbolCommandHandler(
            ISymbolCommandService symbolService,
            ILogger<CreateSymbolCommandHandler> logger)
        {
            _symbolService = symbolService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            CreateSymbolCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating TraderSymbol: {SymbolName}", request.SymbolName);

                var id = await _symbolService.CreateSymbolAsync(
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
                    "TraderSymbol created: {Id} ({SymbolName})",
                    id, request.SymbolName);

                return Result<Guid>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create TraderSymbol: {SymbolName}",
                    request.SymbolName);
                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}