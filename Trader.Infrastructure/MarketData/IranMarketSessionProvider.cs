using Trader.Application.Abstractions.MarketData;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.MarketData;

public sealed class IranMarketSessionProvider : IMarketSessionProvider
{
    private static readonly TimeZoneInfo TehranTz = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

    public bool IsMarketOpen(TradingSchedule? schedule, DateTime utcNow)
    {
        // ─── Fallback: بدون schedule → همیشه باز ───
        if (schedule is null)
            return true;

        // ─── روز تعطیل؟ ───
        var tehran = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TehranTz);
        if (tehran.DayOfWeek is DayOfWeek.Thursday or DayOfWeek.Friday)
            return false;

        var t = TimeOnly.FromTimeSpan(tehran.TimeOfDay);

        // ─── جلسه‌ی اصلی (شامل پیش‌گشایش) ───
        if (t >= schedule.PreMarketStart && t <= schedule.SessionEnd)
            return true;

        // ─── جلسه‌ی دوم (اختیاری) ───
        if (schedule.AfternoonStart.HasValue
            && schedule.AfternoonEnd.HasValue
            && t >= schedule.AfternoonStart.Value
            && t <= schedule.AfternoonEnd.Value)
        {
            return true;
        }

        // ─── After-Hours (اختیاری — مثلاً عیار تا 18) ───
        if (schedule.AfterHoursEnd.HasValue
            && t >= schedule.SessionEnd
            && t <= schedule.AfterHoursEnd.Value)
        {
            return true;
        }

        return false;
    }

    public DateTime GetNextMarketOpen(TradingSchedule? schedule, DateTime utcNow)
    {
        // ─── Fallback: همیشه باز → الان ───
        if (schedule is null)
            return utcNow;

        var tehran = TimeZoneInfo.ConvertTimeFromUtc(utcNow, TehranTz);
        var today = tehran.Date;
        var t = TimeOnly.FromTimeSpan(tehran.TimeOfDay);
        var isTradingDay = tehran.DayOfWeek is not (DayOfWeek.Thursday or DayOfWeek.Friday);

        // ─── امروز پیش از پیش‌گشایش ───
        if (isTradingDay && t < schedule.PreMarketStart)
            return ConvertTehranToUtc(today + schedule.PreMarketStart.ToTimeSpan());

        // ─── امروز بین دو جلسه ───
        if (isTradingDay
            && schedule.AfternoonStart.HasValue
            && t >= schedule.SessionEnd
            && t < schedule.AfternoonStart.Value)
        {
            return ConvertTehranToUtc(today + schedule.AfternoonStart.Value.ToTimeSpan());
        }

        // ─── فردا یا بعد (با skip پنجشنبه/جمعه) ───
        var nextDay = today.AddDays(1);
        while (nextDay.DayOfWeek is DayOfWeek.Thursday or DayOfWeek.Friday)
            nextDay = nextDay.AddDays(1);

        return ConvertTehranToUtc(nextDay + schedule.PreMarketStart.ToTimeSpan());
    }

    private static DateTime ConvertTehranToUtc(DateTime tehranLocal)
    {
        var unspecified = DateTime.SpecifyKind(tehranLocal, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, TehranTz);
    }
}