using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.CurrencyRates;

/// <summary>
/// آخرین نرخ یک Pair قبل از یک زمان مشخص.
/// ORDER BY EffectiveAt DESC LIMIT 1 — از Index DESC استفاده می‌کند.
/// </summary>
public sealed class LatestCurrencyRateSpec : BaseSpecification<CurrencyRate>
{
    public LatestCurrencyRateSpec(
        string fromCurrency,
        string toCurrency,
        DateTime atTime)
        : base(r => r.FromCurrency == fromCurrency
                 && r.ToCurrency == toCurrency
                 && r.EffectiveAt <= atTime)
    {
        ApplyOrderByDescending(r => r.EffectiveAt);
        ApplyPaging(0, 1);
    }
}