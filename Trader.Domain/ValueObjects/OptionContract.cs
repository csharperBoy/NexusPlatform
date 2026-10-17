using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.ValueObjects;

public sealed class OptionContract : ValueObject
{
    public OptionType Type { get; private set; }
    public Price StrikePrice { get; private set; }
    public DateOnly ExpirationDate { get; private set; }
    public string UnderlyingIsin { get; private set; } = default!;

    private OptionContract() { } // EF

    public static OptionContract Create(
        OptionType type,
        Price strikePrice,
        DateOnly expirationDate,
        string underlyingIsin)
    {
        if (string.IsNullOrWhiteSpace(underlyingIsin))
            throw new ArgumentException("Underlying ISIN is required.", nameof(underlyingIsin));

        return new()
        {
            Type = type,
            StrikePrice = strikePrice,
            ExpirationDate = expirationDate,
            UnderlyingIsin = underlyingIsin
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return StrikePrice;
        yield return ExpirationDate;
        yield return UnderlyingIsin;
    }
}