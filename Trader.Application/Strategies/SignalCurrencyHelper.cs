using Trader.Application.Abstractions.Strategies;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Strategies;

/// <summary>
/// تبدیل SL/TP سیگنال از currency استراتژی به IRR برای لایه‌ی اجرا.
///
/// چرا لازم است (D55/D57 — Two-Stream Architecture):
///   • Strategy روی کندل‌های transformed اجرا می‌شه (مثلاً USD)
///   • Fill/Monitoring روی کندل‌های IRR اجرا می‌شن
///   • SL/TP که استراتژی در USD تولید می‌کنه، اگر خام ذخیره بشه، در
///     مقایسه با قیمت‌های IRR باعث false trigger فوری می‌شه
///     (مثلاً TP=0.044 USD در برابر High=22,000 IRR → TP فوری)
///
/// Solution: نسبت ratio = irrClose / strategyClose در همان کندل،
/// SL/TP رو به IRR مقیاس می‌ده. ratio مستقل از جهت rate است
/// (USD per IRR یا IRR per USD — فرقی نمی‌کنه).
/// </summary>
public static class SignalCurrencyHelper
{
    /// <summary>
    /// SL/TP سیگنال را از currency استراتژی (مثلاً USD) به IRR تبدیل می‌کند.
    /// اگر سیگنال SL/TP ندارد یا strategyClose نامعتبر است → همان سیگنال برمی‌گردد.
    /// </summary>
    public static StrategySignal ScaleSlTpToIrr(
        StrategySignal signal,
        decimal strategyClose,
        decimal irrClose)
    {
        if (strategyClose <= 0m) return signal;
        if (irrClose <= 0m) return signal;
        if (!signal.StopLoss.HasValue && !signal.TakeProfit.HasValue)
            return signal;

        var ratio = irrClose / strategyClose;

        return signal with
        {
            // Price? → Price (nullable unwrap) → decimal (Price.Value)
            StopLoss = signal.StopLoss.HasValue
                ? new Price(signal.StopLoss.Value.Value * ratio)
                : null,
            TakeProfit = signal.TakeProfit.HasValue
                ? new Price(signal.TakeProfit.Value.Value * ratio)
                : null,
        };
    }
}