namespace Trader.Application.Runtime;

/// <summary>
/// محاسبه‌ی تاریخ تهران از UTC.
/// روز معاملاتی ایران با این helper محاسبه می‌شود.
/// </summary>
public static class TehranDate
{
    private static readonly TimeZoneInfo TehranTz = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

    /// <summary>تاریخ تهران در لحظه‌ی utcNow.</summary>
    public static DateOnly FromUtc(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

        var tehran = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TehranTz);
        return DateOnly.FromDateTime(tehran);
    }

    /// <summary>تاریخ تهران برای حالا.</summary>
    public static DateOnly Today() => FromUtc(DateTime.UtcNow);
}