using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Indicators;

/// <summary>
/// ورودی استاندارد Indicator — همه چیز از قبل آماده.
/// Strategy فیلترهای لازم را قبل از ساخت این اعمال می‌کند.
/// </summary>
public sealed record IndicatorInput(
    IReadOnlyList<Candle> Candles,
    MarketSnapshot LatestSnapshot,
    TimeFrame TimeFrame,

    // ─── Extension Point (Phase 3) ───
    // اگر Strategy بخواهد Indicator را روی سری تبدیل‌شده محاسبه کند
    // (مثلاً Candleهای USD-adjusted)، Transformation قبل از IIndicator
    // اعمال می‌شود. Indicator بی‌خبر از Currency.
    IReadOnlyDictionary<string, object?>? TransformationContext = null);