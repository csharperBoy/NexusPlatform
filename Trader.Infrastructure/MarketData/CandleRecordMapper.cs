using Trader.Application.Runtime;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.MarketData;

public static class CandleRecordMapper
{
    public static CandleRecord FromRuntime(
        Candle src,
        string isin,
        int timeFrame,
        string source,
        DateTime fetchedAt)
    {
        return CandleRecord.Create(
            isin: isin,
            timeFrame: timeFrame,
            openTime: src.OpenTime,
            open: src.Open,
            high: src.High,
            low: src.Low,
            close: src.Close,
            volume: src.Volume,
            source: source,
            fetchedAt: fetchedAt);
    }
}