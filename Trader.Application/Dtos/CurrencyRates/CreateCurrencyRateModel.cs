namespace Trader.Application.Dtos.CurrencyRates;

public sealed record CreateCurrencyRateModel(
    string FromCurrency,
    string ToCurrency,
    decimal Rate,
    DateTime EffectiveAt,
    string Source = "Manual");