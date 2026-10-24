using Trader.Domain.Entities;

namespace Trader.Application.Abstractions.MarketData;

/// <summary>
/// کش ISIN → TradingSchedule. Singleton.
/// با TTL ۱۰ دقیقه. اگر Instrument هیچ Schedule نداشت، DefaultSchedule (از config) استفاده می‌شود.
/// </summary>
public interface ITradingScheduleCache
{
    Task<TradingSchedule?> GetForIsinAsync(string isin, CancellationToken ct = default);

    void Invalidate(string isin);
    void InvalidateAll();
}