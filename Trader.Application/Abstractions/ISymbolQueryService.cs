using Trader.Application.Dtos;

namespace Trader.Application.Abstractions
{
    public interface ISymbolQueryService
    {
        Task<IReadOnlyList<SymbolInfoView>> GetSymbolListAsync();

        Task<MarketSymbolInfoView> GetMarketInfoAsync(string symbolIsin);
    }
}