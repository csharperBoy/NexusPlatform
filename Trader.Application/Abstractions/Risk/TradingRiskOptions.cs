namespace Trader.Application.Abstractions.Risk;

/// <summary>
/// تنظیمات Risk برای همه‌ی Guardها.
/// در PR 14 فقط Kill Switch فعال است — بقیه در PR 15-18 استفاده می‌شوند.
/// </summary>
public sealed class TradingRiskOptions
{
    public const string SectionName = "Trader:Risk";

    /// <summary>Master switch — اگر false، هیچ Guard اجرا نمی‌شود.</summary>
    public bool Enabled { get; set; } = true;

    public DailyLossLimitOptions DailyLossLimit { get; set; } = new();
    public MaxPositionSizeOptions MaxPositionSize { get; set; } = new();
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();
    public RateLimiterOptions RateLimiter { get; set; } = new();
}

public sealed class DailyLossLimitOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>حداکثر ضرر روزانه به‌صورت درصد از Opening Equity.</summary>
    public decimal MaxLossPercent { get; set; } = 5.0m;

    /// <summary>
    /// Run-level: ضرر نسبت به Opening Equity همان Run محاسبه می‌شود.
    /// (Account-level در آینده اگر نیاز بود.)
    /// </summary>
    public string Scope { get; set; } = "Run";
}

public sealed class MaxPositionSizeOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>حداکثر درصد از Run Capital که یک Symbol می‌تواند بگیرد.</summary>
    public decimal MaxPercentPerSymbol { get; set; } = 30.0m;
}

public sealed class CircuitBreakerOptions
{
    public bool Enabled { get; set; } = true;
    public int ConsecutiveLosses { get; set; } = 5;
    public int CooldownMinutes { get; set; } = 60;
}

public sealed class RateLimiterOptions
{
    public bool Enabled { get; set; } = true;
    public int OrdersPerMinutePerAccount { get; set; } = 10;
}