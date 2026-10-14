using Trader.Application.Abstractions.MarketData;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.MarketData;

public sealed class MarketDataProviderResolver : IMarketDataProviderResolver
{
    private readonly IReadOnlyList<IMarketDataProvider> _providers;

    public MarketDataProviderResolver(IEnumerable<IMarketDataProvider> providers)
    {
        _providers = providers.ToList();
    }

    public IMarketDataProvider? Resolve(MarketType market)
        => _providers.FirstOrDefault(p => p.SupportedMarkets.Contains(market));
}