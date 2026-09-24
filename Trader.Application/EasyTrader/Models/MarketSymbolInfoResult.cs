namespace Trader.Application.EasyTrader.Models
{
    public record MarketSymbolInfoResult(
        string SymbolIsin,
        long? HighAllowedPrice,
        long? LowAllowedPrice,
        long? LastTradedPrice,
        long? ClosingPrice,
        long? FirstTradedPrice,
        string? TradeDate,
        long FetchedAt);
}