using Core.Domain.Common;

namespace Trader.Domain.ValueObjects;

public sealed class TakeProfitConfig : ValueObject
{
    public decimal? Percentage { get; private set; }
    public Price? AbsolutePrice { get; private set; }

    private TakeProfitConfig() { } // EF

    public static TakeProfitConfig FromPercentage(decimal percentage)
    {
        if (percentage <= 0m)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        return new() { Percentage = percentage };
    }

    public static TakeProfitConfig FromPrice(Price price)
    {
        if (price.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(price));
        return new() { AbsolutePrice = price };
    }

    public Price ResolveForEntry(Price entryPrice)
    {
        if (AbsolutePrice.HasValue) return AbsolutePrice.Value;
        var tpValue = entryPrice.Value * (1m + (Percentage!.Value / 100m));
        return new Price(tpValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Percentage;
        yield return AbsolutePrice;
    }
}