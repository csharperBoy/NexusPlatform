using System.Text.Json.Serialization;
using Scheduler.Domain.Enums;

namespace Scheduler.Domain.ValueObjects;

/// <summary>
/// الگوی تکرار. Pure Value Object — هیچ I/O.
/// ذخیره می‌شود به‌صورت JSON در RecurringJob.RecurrencePatternJson.
/// </summary>
public sealed class RecurrencePattern
{
    public RecurrenceType Type { get; }
    public int Interval { get; }
    public DayOfWeek[]? DaysOfWeek { get; }
    public int[]? DaysOfMonth { get; }

    [JsonConstructor]
    public RecurrencePattern(
        RecurrenceType type,
        int interval,
        DayOfWeek[]? daysOfWeek,
        int[]? daysOfMonth)
    {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be > 0.");

        Type = type;
        Interval = interval;
        DaysOfWeek = daysOfWeek;
        DaysOfMonth = daysOfMonth;
    }

    /* ═══════════════════ Factories ═══════════════════ */

    public static RecurrencePattern Daily(int interval = 1)
        => new(RecurrenceType.Daily, interval, null, null);

    public static RecurrencePattern Weekly(params DayOfWeek[] days)
        => Weekly(1, days);

    public static RecurrencePattern Weekly(int interval, params DayOfWeek[] days)
    {
        if (days is null || days.Length == 0)
            throw new ArgumentException("At least one day is required.", nameof(days));

        return new(
            RecurrenceType.Weekly,
            interval,
            days.Distinct().OrderBy(d => (int)d).ToArray(),
            null);
    }

    public static RecurrencePattern Monthly(params int[] daysOfMonth)
        => Monthly(1, daysOfMonth);

    public static RecurrencePattern Monthly(int interval, params int[] daysOfMonth)
    {
        if (daysOfMonth is null || daysOfMonth.Length == 0)
            throw new ArgumentException("At least one day is required.", nameof(daysOfMonth));

        foreach (var d in daysOfMonth)
        {
            if (d != -1 && (d < 1 || d > 31))
                throw new ArgumentOutOfRangeException(nameof(daysOfMonth),
                    "Days must be 1-31, or -1 for 'last day of month'.");
        }

        return new(
            RecurrenceType.Monthly,
            interval,
            null,
            daysOfMonth.Distinct().OrderBy(d => d).ToArray());
    }

    /* ═══════════════════ Core Logic ═══════════════════ */

    /// <summary>
    /// زمان رخداد بعدی، بر اساس:
    /// - anchorUtc: StartAt پلن (زمان رخداد اول + time-of-day مرجع)
    /// - afterUtc: الان (بعد از اجرای قبلی)
    /// - tz: timezone پلن (پیش‌فرض Asia/Tehran)
    /// خروجی: UTC
    /// </summary>
    public DateTimeOffset GetNextOccurrence(
        DateTimeOffset anchorUtc,
        DateTimeOffset afterUtc,
        TimeZoneInfo tz)
    {
        var anchorLocal = TimeZoneInfo.ConvertTime(anchorUtc, tz);
        var afterLocal = TimeZoneInfo.ConvertTime(afterUtc, tz);

        if (afterLocal < anchorLocal) afterLocal = anchorLocal;

        var nextLocalDateTime = Type switch
        {
            RecurrenceType.Daily => GetNextDailyLocal(anchorLocal.DateTime, afterLocal.DateTime),
            RecurrenceType.Weekly => GetNextWeeklyLocal(anchorLocal.DateTime, afterLocal.DateTime),
            RecurrenceType.Monthly => GetNextMonthlyLocal(anchorLocal.DateTime, afterLocal.DateTime),
            _ => throw new InvalidOperationException($"Unknown RecurrenceType: {Type}")
        };

        var unspecified = DateTime.SpecifyKind(nextLocalDateTime, DateTimeKind.Unspecified);
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
        return new DateTimeOffset(utcDateTime, TimeSpan.Zero);
    }

    /* ═══════════════════ Private Helpers ═══════════════════ */

    private DateTime GetNextDailyLocal(DateTime anchor, DateTime after)
    {
        var timeOfDay = anchor.TimeOfDay;
        var anchorDate = anchor.Date;
        var afterDate = after.Date;

        var daysDiff = (afterDate - anchorDate).Days;

        int nextDayIndex;
        if (after.TimeOfDay >= timeOfDay)
            nextDayIndex = daysDiff + 1;   // today's slot passed → next interval
        else
            nextDayIndex = daysDiff;        // today's slot still ahead

        if (nextDayIndex < Interval) nextDayIndex = Interval;
        if (nextDayIndex % Interval != 0)
            nextDayIndex = ((nextDayIndex / Interval) + 1) * Interval;

        return anchorDate.AddDays(nextDayIndex).Add(timeOfDay);
    }

    private DateTime GetNextWeeklyLocal(DateTime anchor, DateTime after)
    {
        if (DaysOfWeek is null || DaysOfWeek.Length == 0)
            throw new InvalidOperationException("Weekly pattern requires DaysOfWeek.");

        var timeOfDay = anchor.TimeOfDay;
        var anchorWeekStart = anchor.Date.AddDays(-(int)anchor.Date.DayOfWeek);
        var afterDate = after.Date;
        var maxSearch = afterDate.AddDays(7 * Interval + 7);

        for (var date = afterDate; date <= maxSearch; date = date.AddDays(1))
        {
            if (!DaysOfWeek.Contains(date.DayOfWeek)) continue;

            var weekStart = date.AddDays(-(int)date.DayOfWeek);
            var weeksDiff = (int)Math.Round((weekStart - anchorWeekStart).TotalDays / 7);

            if (weeksDiff < 0) continue;
            if (weeksDiff % Interval != 0) continue;

            var candidate = date.Add(timeOfDay);
            if (candidate > after) return candidate;
        }

        throw new InvalidOperationException("Could not find next weekly occurrence.");
    }

    private DateTime GetNextMonthlyLocal(DateTime anchor, DateTime after)
    {
        if (DaysOfMonth is null || DaysOfMonth.Length == 0)
            throw new InvalidOperationException("Monthly pattern requires DaysOfMonth.");

        var timeOfDay = anchor.TimeOfDay;
        var anchorMonth = new DateTime(anchor.Year, anchor.Month, 1);
        var currentMonth = new DateTime(after.Year, after.Month, 1);
        var maxSearch = Interval * 24;

        for (int monthOffset = 0; monthOffset <= maxSearch; monthOffset++)
        {
            var month = currentMonth.AddMonths(monthOffset);
            var monthsDiff = (month.Year - anchorMonth.Year) * 12 + (month.Month - anchorMonth.Month);

            if (monthsDiff < 0) continue;
            if (monthsDiff % Interval != 0) continue;

            var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
            var expandedDays = DaysOfMonth
                .Select(d => d == -1 ? daysInMonth : d)
                .Where(d => d <= daysInMonth)
                .OrderBy(d => d);

            foreach (var day in expandedDays)
            {
                var candidate = new DateTime(month.Year, month.Month, day).Add(timeOfDay);
                if (candidate > after) return candidate;
            }
        }

        throw new InvalidOperationException("Could not find next monthly occurrence.");
    }

    /* ═══════════════════ JSON Round-trip ═══════════════════ */

    public string ToJson()
        => System.Text.Json.JsonSerializer.Serialize(this, JsonOptions);

    public static RecurrencePattern FromJson(string json)
        => System.Text.Json.JsonSerializer.Deserialize<RecurrencePattern>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize RecurrencePattern.");

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
}