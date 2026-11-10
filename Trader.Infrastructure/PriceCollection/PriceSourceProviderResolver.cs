using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.PriceCollection;

public sealed class PriceSourceProviderResolver : IPriceSourceProviderResolver
{
    private readonly IReadOnlyDictionary<PriceSourceType, IPriceSourceProvider> _providers;
    private readonly ILogger<PriceSourceProviderResolver> _logger;

    public PriceSourceProviderResolver(
        IEnumerable<IPriceSourceProvider> providers,
        ILogger<PriceSourceProviderResolver> logger)
    {
        _providers = providers.ToDictionary(p => p.SourceType);
        _logger = logger;

        _logger.LogInformation(
            "PriceSourceProviderResolver initialized with {Count} providers: {Keys}",
            _providers.Count,
            string.Join(", ", _providers.Keys));
    }

    public IPriceSourceProvider? Resolve(PriceSourceType type)
        => _providers.TryGetValue(type, out var p) ? p : null;
}