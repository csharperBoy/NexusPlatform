using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.MarketData;

public interface IMarketDataProviderResolver
{
    /// <summary>Provider مناسب برای یک Market. اگر پیدا نشود، null.</summary>
    IMarketDataProvider? Resolve(MarketType market);
}