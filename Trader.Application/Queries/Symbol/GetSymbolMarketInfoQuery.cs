using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Queries.Symbol
{
    public record GetSymbolMarketInfoQuery(string SymbolIsin)
        : IRequest<Result<MarketSymbolInfoView>>;

    public class GetSymbolMarketInfoQueryHandler
        : IRequestHandler<GetSymbolMarketInfoQuery, Result<MarketSymbolInfoView>>
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

        public async Task<Result<MarketSymbolInfoView>> Handle(
            GetSymbolMarketInfoQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Getting Market Info for symbol {Isin}", request.SymbolIsin);

                var info = await _symbolQueryService.GetMarketInfoAsync(request.SymbolIsin);
                return Result<MarketSymbolInfoView>.Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to get Market Info for {Isin}", request.SymbolIsin);
                return Result<MarketSymbolInfoView>.Fail(ex.Message);
            }
        }
    }
}