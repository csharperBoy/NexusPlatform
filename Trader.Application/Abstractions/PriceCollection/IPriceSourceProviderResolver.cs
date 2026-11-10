using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.PriceCollection;

public interface IPriceSourceProviderResolver
{
    IPriceSourceProvider? Resolve(PriceSourceType type);
}