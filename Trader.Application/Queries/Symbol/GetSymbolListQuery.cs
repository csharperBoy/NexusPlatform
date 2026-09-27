 
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.Symbol
{
    public record GetSymbolListQuery(string? Search = null)
        : IRequest<Result<IReadOnlyList<SymbolInfoView>>>;

    public class GetSymbolListQueryHandler
        : IRequestHandler<GetSymbolListQuery, Result<IReadOnlyList<SymbolInfoView>>>
    {
        private readonly ISymbolQueryService _symbolQueryService;
        private readonly ILogger<GetSymbolListQueryHandler> _logger;

        public GetSymbolListQueryHandler(
            ISymbolQueryService symbolQueryService,
            ILogger<GetSymbolListQueryHandler> logger)
        {
            _symbolQueryService = symbolQueryService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<SymbolInfoView>>> Handle(
            GetSymbolListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Trader Symbol List");

                var symbols = await _symbolQueryService.GetSymbolListAsync();
                return Result<IReadOnlyList<SymbolInfoView>>.Ok(symbols);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Trader Symbol List");
                return Result<IReadOnlyList<SymbolInfoView>>.Fail(ex.Message);
            }
        }
    }
}