using Core.Domain.Common;

namespace Trader.Domain.ValueObjects;

/// <summary>حد سود: درصدی یا قیمت مطلق.</summary>
public sealed class TakeProfitConfig : ValueObject
{
    public decimal? Percentage { get; }
    public Price? AbsolutePrice { get; }

    private TakeProfitConfig(decimal? percentage, Price? absolutePrice)
    {
        Percentage = percentage;
        AbsolutePrice = absolutePrice;
    }

    public static TakeProfitConfig FromPercentage(decimal percentage)
    {
        if (percentage <= 0m)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        return new TakeProfitConfig(percentage, null);
    }

    public static TakeProfitConfig FromPrice(Price price)
    {
        if (price.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(price));
        return new TakeProfitConfig(null, price);
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