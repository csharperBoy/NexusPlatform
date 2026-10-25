namespace Trader.Application.Dtos.CurrencyRates;

public sealed record CurrencyRateView(
    Guid Id,
    string FromCurrency,
    string ToCurrency,
    decimal Rate,
    DateTime EffectiveAt,
    string Source,
    DateTime CreatedAt);