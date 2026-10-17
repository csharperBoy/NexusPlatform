using Core.Domain.Common;

namespace Trader.Domain.ValueObjects;

public sealed class StopLossConfig : ValueObject
{
    public decimal? Percentage { get; private set; }
    public Price? AbsolutePrice { get; private set; }

    private StopLossConfig() { } // EF

    public static StopLossConfig FromPercentage(decimal percentage)
    {
        if (percentage <= 0m)
            throw new ArgumentOutOfRangeException(nameof(percentage));
        return new() { Percentage = percentage };
    }

    public static StopLossConfig FromPrice(Price price)
    {
        if (price.Value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(price));
        return new() { AbsolutePrice = price };
    }

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