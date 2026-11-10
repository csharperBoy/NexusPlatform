using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.CurrencyRates;

public sealed class CurrencyRatesPairRangeSpec : BaseSpecification<CurrencyRate>
{
    public CurrencyRatesPairRangeSpec(
        string fromCurrency, string toCurrency,
        DateTime fromTime, DateTime toTime)
        : base(r => r.FromCurrency == fromCurrency
                 && r.ToCurrency == toCurrency
                 && r.EffectiveAt >= fromTime
                 && r.EffectiveAt <= toTime)
    {
    }
}