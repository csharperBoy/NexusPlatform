using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// ثبت یک Candle بسته‌شده. Unique روی (Isin, TimeFrame, OpenTime).
/// برای Historical Ground Truth و Backtesting آینده.
/// </summary>
public class CandleRecord : BaseEntity
{
    public string Isin { get; private set; } = default!;
    public int TimeFrame { get; private set; }         // TimeFrame enum as int
    public DateTime OpenTime { get; private set; }     // UTC

    public decimal Open { get; private set; }
    public decimal High { get; private set; }
    public decimal Low { get; private set; }
    public decimal Close { get; private set; }
    public long Volume { get; private set; }

    public string Source { get; private set; } = "";
    public DateTime FetchedAt { get; private set; }

    private CandleRecord() { } // EF

    public static CandleRecord Create(
        string isin,
        int timeFrame,
        DateTime openTime,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume,
        string source,
        DateTime fetchedAt)
    {
        return new CandleRecord
        {
            Isin = isin,
            TimeFrame = timeFrame,
            OpenTime = openTime,
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
            Source = source,
            FetchedAt = fetchedAt
        };
    }
}