using Trader.Domain.Entities;

namespace Trader.Application.Abstractions.MarketData;

/// <summary>
/// ساعت معاملاتی بر اساس TradingSchedule.
/// schedule == null → "همیشه باز" (fallback).
/// زمان‌ها به UTC برمی‌گردند.
/// </summary>
public interface IMarketSessionProvider
{
    /// <summary>آیا بازار در این لحظه باز است؟</summary>
    bool IsMarketOpen(TradingSchedule? schedule, DateTime utcNow);

    /// <summary>
    /// زمان باز شدن بعدی بازار. اگر الان باز است یا schedule null است، utcNow.
    /// </summary>
    DateTime GetNextMarketOpen(TradingSchedule? schedule, DateTime utcNow);
}