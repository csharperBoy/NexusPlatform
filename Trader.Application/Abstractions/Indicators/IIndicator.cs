namespace Trader.Application.Abstractions.Indicators;

/// <summary>
/// قرارداد پایه برای Indicatorهای خالص (RSI, MACD, EMA, ...).
/// هیچ I/O، هیچ وابستگی به Strategy.
/// </summary>
public interface IIndicator<TConfig, TResult> where TConfig : class
{
    /// <summary>شناسه‌ی یکتا — مثلاً "rsi", "macd", "ema20".</summary>
    string Key { get; }

    string DisplayName { get; }

    /// <summary>
    /// محاسبه‌ی Indicator. Sync — هیچ I/O ندارد.
    /// خروجی null یعنی «داده کافی نیست».
    /// </summary>
    TResult Compute(IndicatorInput input, TConfig config);
}