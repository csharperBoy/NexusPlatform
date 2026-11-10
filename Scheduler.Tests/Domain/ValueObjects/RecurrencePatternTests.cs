using FluentAssertions;
using Scheduler.Domain.Enums;
using Scheduler.Domain.ValueObjects;
using Xunit;

namespace Scheduler.Tests.Domain.ValueObjects;

public class RecurrencePatternTests
{
    private static readonly TimeZoneInfo UtcTz = TimeZoneInfo.Utc;
    private static readonly DateTimeOffset Anchor =
        new(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    /* ═══════════════════ Minutely ═══════════════════ */

    [Fact]
    public void Minutely_FirstOccurrence_BeforeAnchor_ReturnsAnchor()
    {
        var p = RecurrencePattern.Minutely(1);
        var before = Anchor.AddSeconds(-1);

        p.GetNextOccurrence(Anchor, before, UtcTz).Should().Be(Anchor);
    }

    [Fact]
    public void Minutely_AfterAtAnchor_ReturnsAnchorPlusOneMinute()
    {
        var p = RecurrencePattern.Minutely(1);

        p.GetNextOccurrence(Anchor, Anchor, UtcTz)
            .Should().Be(Anchor.AddMinutes(1));
    }

    [Fact]
    public void Minutely_AfterBetweenIntervals_RoundsToNextInterval()
    {
        var p = RecurrencePattern.Minutely(1);
        var after = Anchor.AddSeconds(30);

        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(Anchor.AddMinutes(1));
    }

    [Fact]
    public void Minutely_AfterAtExactInterval_ReturnsNextInterval()
    {
        var p = RecurrencePattern.Minutely(1);
        var after = Anchor.AddMinutes(1);

        // strictly-after → next = +2min, نه +1min
        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(Anchor.AddMinutes(2));
    }

    [Fact]
    public void Minutely_Interval5_Adds5Minutes()
    {
        var p = RecurrencePattern.Minutely(5);

        p.GetNextOccurrence(Anchor, Anchor, UtcTz)
            .Should().Be(Anchor.AddMinutes(5));
    }

    [Fact]
    public void Minutely_MultipleIntervalsElapsed_RoundsToNextMultiple()
    {
        var p = RecurrencePattern.Minutely(5);
        var after = Anchor.AddMinutes(12);   // بین 10 و 15

        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(Anchor.AddMinutes(15));
    }

    /* ═══════════════════ Hourly ═══════════════════ */

    [Fact]
    public void Hourly_AfterAtAnchor_ReturnsAnchorPlusOneHour()
    {
        var p = RecurrencePattern.Hourly(1);

        p.GetNextOccurrence(Anchor, Anchor, UtcTz)
            .Should().Be(Anchor.AddHours(1));
    }

    [Fact]
    public void Hourly_Interval4_AddsFourHours()
    {
        var p = RecurrencePattern.Hourly(4);

        p.GetNextOccurrence(Anchor, Anchor, UtcTz)
            .Should().Be(Anchor.AddHours(4));
    }

    [Fact]
    public void Hourly_AfterExactInterval_SkipsToNext()
    {
        var p = RecurrencePattern.Hourly(2);
        var after = Anchor.AddHours(2);

        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(Anchor.AddHours(4));
    }

    /* ═══════════════════ Cron ═══════════════════ */

    [Fact]
    public void Cron_EveryMinute_ReturnsNextMinute()
    {
        var p = RecurrencePattern.Cron("* * * * *");
        var after = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(after.AddMinutes(1));
    }

    [Fact]
    public void Cron_EveryHourAt0_ReturnsNextHour()
    {
        var p = RecurrencePattern.Cron("0 * * * *");
        var after = new DateTimeOffset(2026, 1, 1, 10, 30, 0, TimeSpan.Zero);

        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(new DateTimeOffset(2026, 1, 1, 11, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Cron_DailyAt930_ReturnsNextDay()
    {
        var p = RecurrencePattern.Cron("30 9 * * *");
        var after = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(new DateTimeOffset(2026, 1, 2, 9, 30, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Cron_WithAnchorInFuture_RespectsAnchor()
    {
        var anchor = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var after = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var p = RecurrencePattern.Cron("0 12 * * *");

        // اولین Cron match بعد از anchor = 2026-06-01 12:00
        p.GetNextOccurrence(anchor, after, UtcTz)
            .Should().Be(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));
    }

    /* ═══════════════════ Daily (regression) ═══════════════════ */

    [Fact]
    public void Daily_AfterAtAnchor_ReturnsNextDay()
    {
        var p = RecurrencePattern.Daily(1);

        p.GetNextOccurrence(Anchor, Anchor, UtcTz)
            .Should().Be(Anchor.AddDays(1));
    }

    [Fact]
    public void Daily_Interval3_SkipsTwoDays()
    {
        var p = RecurrencePattern.Daily(3);

        p.GetNextOccurrence(Anchor, Anchor, UtcTz)
            .Should().Be(Anchor.AddDays(3));
    }

    /* ═══════════════════ Weekly (regression) ═══════════════════ */

    [Fact]
    public void Weekly_SingleDay_ReturnsNextWeekSameDay()
    {
        // Anchor = Thursday 2026-01-01 10:00 UTC
        var p = RecurrencePattern.Weekly(DayOfWeek.Thursday);
        var after = Anchor.AddDays(1);   // Friday

        // بعد از Friday، پنجشنبه‌ی بعدی = 2026-01-08
        p.GetNextOccurrence(Anchor, after, UtcTz)
            .Should().Be(new DateTimeOffset(2026, 1, 8, 10, 0, 0, TimeSpan.Zero));
    }

    /* ═══════════════════ Monthly (regression) ═══════════════════ */

    [Fact]
    public void Monthly_Day15_ReturnsNextMonth15()
    {
        var anchor = new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var after = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        
        var p = RecurrencePattern.Monthly(1, 15);
        p.GetNextOccurrence(anchor, after, UtcTz)
            .Should().Be(new DateTimeOffset(2026, 2, 15, 10, 0, 0, TimeSpan.Zero));
    }

    /* ═══════════════════ Validation ═══════════════════ */

    [Fact]
    public void Constructor_ZeroInterval_Throws()
    {
        var act = () => new RecurrencePattern(RecurrenceType.Daily, 0, null, null);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("interval");
    }

    [Fact]
    public void Constructor_CronWithoutExpression_Throws()
    {
        var act = () => new RecurrencePattern(RecurrenceType.Cron, 1, null, null, null);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("cronExpression");
    }

    [Fact]
    public void Constructor_CronExpressionOnNonCron_Throws()
    {
        var act = () => new RecurrencePattern(
            RecurrenceType.Daily, 1, null, null, "0 * * * *");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("cronExpression");
    }

    [Fact]
    public void Constructor_InvalidCronExpression_Throws()
    {
        var act = () => new RecurrencePattern(
            RecurrenceType.Cron, 1, null, null, "not-a-cron");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("cronExpression");
    }

    [Fact]
    public void Cron_FactoryWithEmptyExpression_Throws()
    {
        var act = () => RecurrencePattern.Cron("");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("expression");
    }

    /* ═══════════════════ JSON Round-trip ═══════════════════ */

    [Fact]
    public void Json_DailyPattern_RoundTrips()
    {
        var p = RecurrencePattern.Daily(2);
        var json = p.ToJson();
        var back = RecurrencePattern.FromJson(json);

        back.Type.Should().Be(RecurrenceType.Daily);
        back.Interval.Should().Be(2);
        back.CronExpression.Should().BeNull();
    }

    [Fact]
    public void Json_MinutelyPattern_RoundTrips()
    {
        var p = RecurrencePattern.Minutely(5);
        var back = RecurrencePattern.FromJson(p.ToJson());

        back.Type.Should().Be(RecurrenceType.Minutely);
        back.Interval.Should().Be(5);
    }

    [Fact]
    public void Json_HourlyPattern_RoundTrips()
    {
        var p = RecurrencePattern.Hourly(6);
        var back = RecurrencePattern.FromJson(p.ToJson());

        back.Type.Should().Be(RecurrenceType.Hourly);
        back.Interval.Should().Be(6);
    }

    [Fact]
    public void Json_CronPattern_RoundTrips()
    {
        var p = RecurrencePattern.Cron("30 9 * * 1-5");
        var back = RecurrencePattern.FromJson(p.ToJson());

        back.Type.Should().Be(RecurrenceType.Cron);
        back.CronExpression.Should().Be("30 9 * * 1-5");
    }

    [Fact]
    public void Json_LegacyDailyJson_ParsesWithoutMigration()
    {
        // JSON قدیمی بدون cronExpression field (از ردیف‌های DB قبل از Extension)
        var legacyJson = "{\"type\":0,\"interval\":1,\"daysOfWeek\":null,\"daysOfMonth\":null}";

        var p = RecurrencePattern.FromJson(legacyJson);

        p.Type.Should().Be(RecurrenceType.Daily);
        p.Interval.Should().Be(1);
        p.CronExpression.Should().BeNull();
    }

    [Fact]
    public void Json_LegacyWeeklyJson_ParsesCorrectly()
    {
        var legacyJson =
            "{\"type\":1,\"interval\":1,\"daysOfWeek\":[4],\"daysOfMonth\":null}";

        var p = RecurrencePattern.FromJson(legacyJson);

        p.Type.Should().Be(RecurrenceType.Weekly);
        p.DaysOfWeek.Should().BeEquivalentTo(new[] { DayOfWeek.Thursday });
    }

    /* ═══════════════════ Timezone ═══════════════════ */

    [Fact]
    public void Daily_PreservesLocalTimeOfDay_AcrossTimezones()
    {
        var tehran = TryGetTehranTz();
        if (tehran is null) return;   // skip if not available

        // Anchor = 2026-01-01 20:00 UTC = 2026-01-01 23:30 Tehran
        var anchor = new DateTimeOffset(2026, 1, 1, 20, 0, 0, TimeSpan.Zero);
        var after = anchor.AddHours(1);   // 21:00 UTC = 00:30 Tehran next day

        var p = RecurrencePattern.Daily(1);

        var next = p.GetNextOccurrence(anchor, after, tehran);

        // باید 2026-01-02 23:30 Tehran باشد = 2026-01-02 20:00 UTC
        next.Should().Be(new DateTimeOffset(2026, 1, 2, 20, 0, 0, TimeSpan.Zero));
    }

    // ─── Helper ───
    private static TimeZoneInfo? TryGetTehranTz()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");
        }
        catch
        {
            return null;
        }
    }
}