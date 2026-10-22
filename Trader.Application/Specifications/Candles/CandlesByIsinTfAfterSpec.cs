using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Candles;

/// <summary>
/// کندل‌های موجود بعد از یک زمان مشخص — برای dedup قبل از Insert.
/// </summary>
public sealed class CandlesByIsinTfAfterSpec : BaseSpecification<CandleRecord>
{
    public CandlesByIsinTfAfterSpec(string isin, int timeFrame, DateTime after)
        : base(c => c.Isin == isin
                 && c.TimeFrame == timeFrame
                 && c.OpenTime >= after)
    {
        ApplyOrderBy(c => c.OpenTime);
    }
}