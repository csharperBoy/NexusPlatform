using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;   // ← اضافه

namespace Trader.Application.Specifications.Candles;

public sealed class CandlesByIsinTfAfterSpec : BaseSpecification<CandleRecord>
{
    public CandlesByIsinTfAfterSpec(string isin, int timeFrame, DateTime after)
        : base(c => c.Isin == isin
                 && (int)c.TimeFrame == timeFrame     
                 && c.OpenTime >= after)
    {
        ApplyOrderBy(c => c.OpenTime);
    }
}