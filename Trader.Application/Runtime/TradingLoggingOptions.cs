using Trader.Domain.Enums;

namespace Trader.Application.Runtime;

public sealed class TradingLoggingOptions
{
    public const string SectionName = "Trader:Logging";

    /// <summary>
    /// حداقل سطح رویدادهایی که در DB ذخیره می‌شوند.
    /// مقادیر: Debug / Info / Warning / Error / Critical
    /// </summary>
    public string MinSeverity { get; set; } = "Info";

    /// <summary>
    /// تعداد روزهایی که رویدادها نگه داشته می‌شوند.
    /// </summary>
    public int RetentionDays { get; set; } = 90;

    /// <summary>
    /// حداقل Severity به‌صورت Enum. اگر نامعتبر بود، Info.
    /// </summary>
    public TradingEventSeverity GetMinSeverityEnum()
    {
        return Enum.TryParse<TradingEventSeverity>(MinSeverity, ignoreCase: true, out var result)
            ? result
            : TradingEventSeverity.Info;
    }
}