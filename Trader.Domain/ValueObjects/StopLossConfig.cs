using Core.Domain.Common;

namespace Trader.Domain.ValueObjects;

/// <summary>حد ضرر: درصدی از قیمت ورود، یا قیمت مطلق.</summary>
public sealed class StopLossConfig : ValueObject
{
    public decimal? Percentage { get; }
    public Price? AbsolutePrice { get; }

    private StopLossConfig(decimal? percentage, Price? absolutePrice)
    {
        Percentage = percentage;
        AbsolutePrice = absolutePrice;
    }

    public static StopLossConfig FromPercentage(decimal percentage)
    {
        if (percentage <= 0m)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        return new StopLossConfig(percentage, null);
    }

    public static StopLossConfig FromPrice(Price price)
    {
        if (price.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(price));
        return new StopLossConfig(null, price);
    }

    /// <summary>قیمت حد ضرر بر اساس قیمت ورود.</summary>
    public Price ResolveForEntry(Price entryPrice)
    {
        if (AbsolutePrice.HasValue) return AbsolutePrice.Value;
        var stopValue = entryPrice.Value * (1m - (Percentage!.Value / 100m));
        return new Price(stopValue < 0m ? 0m : stopValue);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Percentage;
        yield return AbsolutePrice;
    }
}