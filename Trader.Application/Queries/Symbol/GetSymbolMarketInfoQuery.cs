using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.Symbol
{
    public record GetSymbolMarketInfoQuery(string SymbolIsin)
        : IRequest<Result<SymbolMarketDataDto>>;

    public class GetSymbolMarketInfoQueryHandler
        : IRequestHandler<GetSymbolMarketInfoQuery, Result<SymbolMarketDataDto>>
    {
        private readonly ISymbolQueryService _symbolQueryService;
        private readonly ILogger<GetSymbolMarketInfoQueryHandler> _logger;

        public GetSymbolMarketInfoQueryHandler(
            ISymbolQueryService symbolQueryService,
            ILogger<GetSymbolMarketInfoQueryHandler> logger)
        {
            _symbolQueryService = symbolQueryService;
            _logger = logger;
        }

        public async Task<Result<SymbolMarketDataDto>> Handle(
            GetSymbolMarketInfoQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Getting Market Info for symbol {Isin}", request.SymbolIsin);

                var info = await _symbolQueryService.GetMarketInfoAsync(request.SymbolIsin);
                return Result<SymbolMarketDataDto>.Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get Market Info for {Isin}", request.SymbolIsin);
                return Result<SymbolMarketDataDto>.Fail(ex.Message);
            }
        }
    }
}