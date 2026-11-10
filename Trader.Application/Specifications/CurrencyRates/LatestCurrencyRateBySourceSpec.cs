using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.CurrencyRates;

/// <summary>
/// آخرین رکورد یک (Pair, Source) قبل از یک زمان مشخص.
/// برای Backfill boundary هر Source مستقل — وقتی چند Source همان Pair را
/// می‌دهند (مثلاً tgju-usd + telegram-usd برای USD/IRR).
///
/// ORDER BY EffectiveAt DESC LIMIT 1 — از Index IX_CurrencyRate_Pair_TimeDesc
/// استفاده می‌کند (Source در Index نیست، ولی در حداکثر چند هزار رکورد/روز
/// فیلتر در memory بعد از Index lookup قابل قبول است).
/// </summary>
public sealed class LatestCurrencyRateBySourceSpec : BaseSpecification<CurrencyRate>
{
    public LatestCurrencyRateBySourceSpec(
        string fromCurrency,
        string toCurrency,
        string source,
        DateTime atTime)
        : base(r => r.FromCurrency == fromCurrency
                 && r.ToCurrency == toCurrency
                 && r.Source == source
                 && r.EffectiveAt <= atTime)
    {
        ApplyOrderByDescending(r => r.EffectiveAt);
        ApplyPaging(0, 1);
    }
}