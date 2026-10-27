using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.CurrencyRates;

/// <summary>
/// همه‌ی نرخ‌های یک Pair (در هر دو جهت) تا لحظه‌ی toTime، صعودی بر اساس EffectiveAt.
/// از این Spec برای Per-Candle Transformation استفاده می‌شود.
/// </summary>
public sealed class CurrencyRatesPairUntilSpec : BaseSpecification<CurrencyRate>
{
    public CurrencyRatesPairUntilSpec(
        string currency1, string currency2, DateTime toTime)
        : base(r =>
            ((r.FromCurrency == currency1 && r.ToCurrency == currency2) ||
             (r.FromCurrency == currency2 && r.ToCurrency == currency1))
            && r.EffectiveAt <= toTime)
    {
        ApplyOrderBy(r => r.EffectiveAt);
    }
}