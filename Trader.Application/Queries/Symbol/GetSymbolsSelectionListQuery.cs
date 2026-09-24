using Core.Shared.DTOs;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Queries.Symbol
{
    public record GetSymbolsSelectionListQuery()
        : IRequest<Result<IList<SelectionListDto>>>;

    public class GetSymbolsSelectionListQueryHandler
        : IRequestHandler<GetSymbolsSelectionListQuery, Result<IList<SelectionListDto>>>
    {
        private readonly ISymbolQueryService _symbolQueryService;
        private readonly ILogger<GetSymbolsSelectionListQueryHandler> _logger;

        public GetSymbolsSelectionListQueryHandler(
            ISymbolQueryService symbolQueryService,
            ILogger<GetSymbolsSelectionListQueryHandler> logger)
        {
            _symbolQueryService = symbolQueryService;
            _logger = logger;
        }

        public async Task<Result<IList<SelectionListDto>>> Handle(
            GetSymbolsSelectionListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var symbols = await _symbolQueryService.GetSymbolListAsync();

                var result = symbols
                    .Select(x => new SelectionListDto(x.SymbolIsin, x.SymbolName))
                    .ToList();

                return Result<IList<SelectionListDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Trader Symbol Selection List");
                return Result<IList<SelectionListDto>>.Fail(ex.Message);
            }
        }
    }
}