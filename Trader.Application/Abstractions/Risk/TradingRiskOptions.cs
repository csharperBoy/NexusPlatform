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

    /// <summary>Run-level (پیش‌فرض) — در آینده اگر Account-level لازم شد.</summary>
    public string Scope { get; set; } = "Run";

    /// <summary>فاصله‌ی Refresh Cash از Broker. بین Refreshها از Cash قدیمی استفاده می‌شود.</summary>
    public TimeSpan BrokerRefreshInterval { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Paper Mode: پیش‌فرض false — چون Paper Cash tracked نیست،
    /// Daily Loss واقعی نیست. اگر true شود، Paper هم Block می‌شود.
    /// </summary>
    public bool ApplyToPaper { get; set; } = false;
}

public sealed class MaxPositionSizeOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>حداکثر درصد از Run Capital که یک Symbol می‌تواند بگیرد.</summary>
    public decimal MaxPercentPerSymbol { get; set; } = 30.0m;

    /// <summary>
    /// Paper Mode: apply or skip?
    /// پیش‌فرض true — Paper باید مانند Live رفتار کند (شبیه‌سازی واقعی‌تر).
    /// با false، Paper Mode از این چک معاف می‌شود.
    /// </summary>
    public bool ApplyToPaper { get; set; } = true;
}

public sealed class CircuitBreakerOptions
{
    public bool Enabled { get; set; } = true;
    public int ConsecutiveLosses { get; set; } = 5;
    public int CooldownMinutes { get; set; } = 60;
    public bool ApplyToPaper { get; set; } = false;
}

public sealed class RateLimiterOptions
{
    public bool Enabled { get; set; } = true;
    public int OrdersPerMinutePerAccount { get; set; } = 10;

    /// <summary>پنجره‌ی rate limit (پیش‌فرض ۶۰ ثانیه). برای تست قابل تنظیم است.</summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>Paper Mode: پیش‌فرض false — Paper broker API ندارد.</summary>
    public bool ApplyToPaper { get; set; } = false;
}