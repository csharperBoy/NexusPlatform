using Core.Domain.Common;
using Trader.Domain.Enums;

namespace Trader.Domain.ValueObjects;

/// <summary>قرارداد اختیار معامله. برای Instrumentهای Option.</summary>
public sealed class OptionContract : ValueObject
{
    public OptionType Type { get; }
    public Price StrikePrice { get; }
    public DateOnly ExpirationDate { get; }
    public string UnderlyingIsin { get; }

    private OptionContract(
        OptionType type,
        Price strikePrice,
        DateOnly expirationDate,
        string underlyingIsin)
    {
        if (string.IsNullOrWhiteSpace(underlyingIsin))
            throw new ArgumentException("Underlying ISIN is required.", nameof(underlyingIsin));
        Type = type;
        StrikePrice = strikePrice;
        ExpirationDate = expirationDate;
        UnderlyingIsin = underlyingIsin;
    }

    public static OptionContract Create(
        OptionType type,
        Price strikePrice,
        DateOnly expirationDate,
        string underlyingIsin)
        => new(type, strikePrice, expirationDate, underlyingIsin);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return StrikePrice;
        yield return ExpirationDate;
        yield return UnderlyingIsin;
    }
}