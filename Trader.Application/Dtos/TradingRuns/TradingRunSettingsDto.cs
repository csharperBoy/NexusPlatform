namespace Trader.Application.Dtos.TradingRuns;

/// <summary>
/// تنظیمات Run که به Account مربوط نیست. Capital Allocation حالا
/// per-account است (در TradingRunAccountDto).
/// </summary>
public sealed record TradingRunSettingsDto(
    decimal? StopLossPercentage,
    decimal? StopLossPrice,
    decimal? TakeProfitPercentage,
    decimal? TakeProfitPrice,
    int DefaultOrderMode,
    int OnMarketClose,
    int OnStop);