 
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.Symbol
{
    public record DeleteSymbolCommand(Guid Id) : IRequest<Result<bool>>;

    public class DeleteSymbolCommandHandler
        : IRequestHandler<DeleteSymbolCommand, Result<bool>>
    {
        private readonly ISymbolCommandService _symbolService;
        private readonly ILogger<DeleteSymbolCommandHandler> _logger;

        public DeleteSymbolCommandHandler(
            ISymbolCommandService symbolService,
            ILogger<DeleteSymbolCommandHandler> logger)
        {
            _symbolService = symbolService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DeleteSymbolCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting TraderSymbol: {Id}", request.Id);

                await _symbolService.DeleteSymbolAsync(request.Id);
                await _symbolService.SaveAsync();

                _logger.LogInformation(
                    "TraderSymbol deleted: {Id}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to delete TraderSymbol: {Id}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}