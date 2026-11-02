namespace Trader.Domain.Enums;

public enum BacktestExitReason
{
    Signal = 0,          // سیگنال فروش از Strategy
    StopLoss = 1,
    TakeProfit = 2,
    EndOfBacktest = 3,   // بسته شدن موقعیت‌های باز در انتهای Backtest
    ManualClose = 4,     // (رزرو — Phase 2)
}