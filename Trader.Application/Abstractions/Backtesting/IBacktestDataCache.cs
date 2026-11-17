using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// Cache برای داده‌های ثابتِ simulation (candles + instruments).
///
/// Scope:
///   • BacktestEngine — Scoped، pass-through (یک‌بار load، یک‌بار استفاده)
///   • Tuning — Scoped به tuning run، share بین هزاران simulation (بزرگ‌ترین برد)
///
/// Thread-safe: ConcurrentDictionary + Lazy Task.
/// Cache lifetime = DI scope lifetime.
/// </summary>
public interface IBacktestDataCache
{
    Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string isin, TimeFrame timeFrame,
        DateTime from, DateTime to, CancellationToken ct = default);

    Task<IReadOnlyList<Candle>> GetTransformedCandlesAsync(
        string isin, TimeFrame timeFrame,
        DateTime from, DateTime to, string targetCurrency,
        CancellationToken ct = default);

    Task<Instrument> GetInstrumentAsync(string isin, CancellationToken ct = default);
}