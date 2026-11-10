using System.Text.Json.Serialization;
using Cronos;
using Scheduler.Domain.Enums;

namespace Scheduler.Domain.ValueObjects;

/// <summary>
/// الگوی تکرار. Pure Value Object — هیچ I/O.
/// ذخیره می‌شود به‌صورت JSON در RecurringSchedule.RecurrencePatternJson.
///
/// انواع:
///   • Minutely/Houly  — Interval به دقیقه/ساعت (Real-time, DST-free)
///   • Daily/Weekly/Monthly — Anchor time-of-day + tz (DST-aware)
///   • Cron — Cron expression (5-field standard, via Cronos)
///
/// Backward Compat: enum values 0/1/2 برای Daily/Weekly/Monthly بدون تغییر.
/// </summary>
public sealed class RecurrencePattern
{
    public RecurrenceType Type { get; }
    public int Interval { get; }
    public DayOfWeek[]? DaysOfWeek { get; }
    public int[]? DaysOfMonth { get; }

    /// <summary>فقط برای Type=Cron. 5-field expression (minute hour day month weekday).</summary>
    public string? CronExpression { get; }

    [JsonConstructor]
    public RecurrencePattern(
        RecurrenceType type,
        int interval,
        DayOfWeek[]? daysOfWeek,
        int[]? daysOfMonth,
        string? cronExpression = null)
    {
        if (interval <= 0)
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be > 0.");

        // ─── Cron validation ───
        if (type == RecurrenceType.Cron)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                throw new ArgumentException(
                    "CronExpression is required when Type = Cron.",
                    nameof(cronExpression));

            try
            {
                Cronos.CronExpression.Parse(cronExpression);
            }
            catch (CronFormatException ex)
            {
                throw new ArgumentException(
                    $"Invalid cron expression '{cronExpression}': {ex.Message}",
                    nameof(cronExpression), ex);
            }
        }
        else if (!string.IsNullOrWhiteSpace(cronExpression))
        {
            throw new ArgumentException(
                $"CronExpression is only valid when Type = Cron (got {type}).",
                nameof(cronExpression));
        }

        Type = type;
        Interval = interval;
        DaysOfWeek = daysOfWeek;
        DaysOfMonth = daysOfMonth;
        CronExpression = cronExpression?.Trim();
    }

    /* ═══════════════════ Factories ═══════════════════ */

    public static RecurrencePattern Minutely(int interval = 1)
        => new(RecurrenceType.Minutely, interval, null, null);

    public static RecurrencePattern Hourly(int interval = 1)
        => new(RecurrenceType.Hourly, interval, null, null);

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

    /// <summary>
    /// Cron expression (5-field standard: minute hour day-of-month month day-of-week).
    /// مثال‌ها: "*/5 * * * *" (هر ۵ دقیقه), "0 9 * * 1-5" (روزهای هفته ۹:۰۰), "0 23 1 * *" (روز اول هر ماه ۲۳:۰۰).
    /// </summary>
    public static RecurrencePattern Cron(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Cron expression is required.", nameof(expression));

        return new(RecurrenceType.Cron, interval: 1, null, null, expression);
    }

    /* ═══════════════════ Core Logic ═══════════════════ */

    /// <summary>
    /// زمان رخداد بعدی.
    /// Semantics: strictly after afterUtc؛ اگر afterUtc قبل از anchorUtc باشد،
    /// اولین رخداد همان anchorUtc است (به‌جز Cron که anchor معنایی ندارد و از anchor - 1 tick
    /// شروع می‌کند تا اولین Cron match بعد از anchor پیدا شود).
    /// </summary>
    public DateTimeOffset GetNextOccurrence(
        DateTimeOffset anchorUtc,
        DateTimeOffset afterUtc,
        TimeZoneInfo tz)
    {
        // ─── Minutely / Hourly: Interval-based، بدون tz ───
        if (Type == RecurrenceType.Minutely)
            return GetNextSubDaily(anchorUtc, afterUtc, TimeSpan.FromMinutes(Interval));

        if (Type == RecurrenceType.Hourly)
            return GetNextSubDaily(anchorUtc, afterUtc, TimeSpan.FromHours(Interval));

        // ─── Cron: via Cronos ───
        if (Type == RecurrenceType.Cron)
        {
            var cron = Cronos.CronExpression.Parse(CronExpression!);

            // اگر after < anchor → اولین match بعد از anchor - 1tick (تا anchor هم قابل پیدا شدن باشد)
            DateTime fromUtc = afterUtc.UtcDateTime;
            if (afterUtc < anchorUtc)
                fromUtc = anchorUtc.UtcDateTime.AddTicks(-1);

            var next = cron.GetNextOccurrence(fromUtc, tz)
                ?? throw new InvalidOperationException(
                    $"Cron expression '{CronExpression}' has no next occurrence after {afterUtc:O} (tz={tz.Id}).");

            return new DateTimeOffset(next, TimeSpan.Zero);
        }

        // ─── Daily / Weekly / Monthly (منطق موجود) ───
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

    /// <summary>
    /// Next Occurrence برای Minutely/Hourly — Strictly-After semantic،
    /// هم‌راستا با Daily (اگر after == anchor + k*interval، next = anchor + (k+1)*interval).
    /// </summary>
    private static DateTimeOffset GetNextSubDaily(
        DateTimeOffset anchorUtc,
        DateTimeOffset afterUtc,
        TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero)
            throw new InvalidOperationException("Interval must be positive.");

        if (afterUtc < anchorUtc)
            return anchorUtc;

        var elapsedTicks = (afterUtc - anchorUtc).Ticks;
        var intervalTicks = interval.Ticks;

        // elapsedTicks >= 0 پس / = floor. +1 = strictly-after.
        var k = (elapsedTicks / intervalTicks) + 1;
        return anchorUtc.AddTicks(k * intervalTicks);
    }

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