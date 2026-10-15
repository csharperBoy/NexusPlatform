using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.MarketData;

/// <summary>
/// کش کندل‌های بسته‌شده (Historical). 
/// کندل جاری (in-progress) در این Cache نگه‌داری نمی‌شود.
/// </summary>
public interface ICandleCache
{
    /// <summary>
    /// دریافت کندل‌های بسته‌شده برای یک (Isin, TimeFrame).
    /// اگر Cache معتبر باشد، همان را برمی‌گرداند.
    /// اگر نه، از Provider می‌گیرد و Cache می‌کند.
    /// </summary>
    Task<IReadOnlyList<Candle>> GetOrLoadAsync(
        string isin,
        TimeFrame timeFrame,
        int requiredCount,
        CancellationToken ct = default);

    /// <summary>باطل کردن Cache (برای Recovery یا Update اجباری).</summary>
    void Invalidate(string isin, TimeFrame timeFrame);

    void InvalidateAll();
}