using FluentAssertions;
using Trader.Application.Runtime;
using Trader.Application.Strategies.MtfComposite;
using Trader.Domain.Enums;
using Xunit;

namespace Trader.Tests.Application.Strategies;

public class SubCandleAnalyzerTests
{
    private static Candle MakeSub(DateTime open, long volume) =>
        new(open, 10m, 11m, 9m, 10m, volume);

    [Fact]
    public void Analyze_EmptySubCandles_ReturnsNull()
    {
        var result = SubCandleAnalyzer.Analyze(
            Array.Empty<Candle>(),
            TimeSpan.FromMinutes(5),
            DateTime.UtcNow.AddHours(-1),
            TimeSpan.FromHours(1),
            DateTime.UtcNow,
            openingBaselineDays: 30,
            intraDayBaselineCandles: 100,
            minSamples: 10,
            triggerThreshold: 2.0m);

        result.Should().BeNull();
    }

    [Fact]
    public void Analyze_NotEnoughHistory_ReturnsNull()
    {
        // Only 2 historical candles — way below minSamples
        var sub = new List<Candle>
        {
            MakeSub(DateTime.UtcNow.AddHours(-3), 1000),
            MakeSub(DateTime.UtcNow.AddHours(-2), 1000),
            MakeSub(DateTime.UtcNow.AddMinutes(-20), 500), // current
        };

        var result = SubCandleAnalyzer.Analyze(
            sub,
            TimeSpan.FromMinutes(5),
            DateTime.UtcNow.AddHours(-1), // current primary candle start
            TimeSpan.FromHours(1),
            DateTime.UtcNow,
            openingBaselineDays: 30,
            intraDayBaselineCandles: 100,
            minSamples: 10,
            triggerThreshold: 2.0m);

        result.Should().BeNull();
    }

    [Fact]
    public void Analyze_NormalVolume_NotTriggered()
    {
        var now = DateTime.UtcNow;
        var currentPrimaryStart = new DateTime(
            now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

        var sub = new List<Candle>();

        // 20 historical hourly groups, 6 M5 each, ~1000 volume per group
        for (int h = 20; h >= 1; h--)
        {
            var hourStart = currentPrimaryStart.AddHours(-h);
            for (int m = 0; m < 6; m++)
            {
                sub.Add(MakeSub(hourStart.AddMinutes(m * 5), 166)); // ~1000 total
            }
        }

        // Current: 3 M5s closed (15 min), ~500 volume
        for (int m = 0; m < 3; m++)
        {
            sub.Add(MakeSub(currentPrimaryStart.AddMinutes(m * 5), 166));
        }

        var nowInHalf = currentPrimaryStart.AddMinutes(15);

        var result = SubCandleAnalyzer.Analyze(
            sub,
            TimeSpan.FromMinutes(5),
            currentPrimaryStart,
            TimeSpan.FromHours(1),
            nowInHalf,
            openingBaselineDays: 30,
            intraDayBaselineCandles: 100,
            minSamples: 10,
            triggerThreshold: 2.0m);

        result.Should().NotBeNull();
        result!.Value.Triggered.Should().BeFalse();
    }

    [Fact]
    public void Analyze_HighVolume_Triggered()
    {
        var now = DateTime.UtcNow;
        var currentPrimaryStart = new DateTime(
            now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

        var sub = new List<Candle>();

        // Historical: 20 hourly groups, each 6 M5 × 166 volume = ~1000
        for (int h = 20; h >= 1; h--)
        {
            var hourStart = currentPrimaryStart.AddHours(-h);
            for (int m = 0; m < 6; m++)
                sub.Add(MakeSub(hourStart.AddMinutes(m * 5), 166));
        }

        // Current: 3 M5s, but each × 3000 = very high
        for (int m = 0; m < 3; m++)
        {
            sub.Add(MakeSub(currentPrimaryStart.AddMinutes(m * 5), 3000));
        }

        var nowInHalf = currentPrimaryStart.AddMinutes(15);

        var result = SubCandleAnalyzer.Analyze(
            sub,
            TimeSpan.FromMinutes(5),
            currentPrimaryStart,
            TimeSpan.FromHours(1),
            nowInHalf,
            openingBaselineDays: 30,
            intraDayBaselineCandles: 100,
            minSamples: 10,
            triggerThreshold: 2.0m);

        result.Should().NotBeNull();
        result!.Value.Triggered.Should().BeTrue();
    }

    [Fact]
    public void Analyze_NotInFirstHalf_ReturnsNull()
    {
        var now = DateTime.UtcNow;
        var currentPrimaryStart = new DateTime(
            now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);

        var sub = new List<Candle>();
        for (int h = 20; h >= 1; h--)
        {
            var hourStart = currentPrimaryStart.AddHours(-h);
            for (int m = 0; m < 6; m++)
                sub.Add(MakeSub(hourStart.AddMinutes(m * 5), 166));
        }

        // Now is 40 min into candle — past half
        var nowInSecondHalf = currentPrimaryStart.AddMinutes(40);

        var result = SubCandleAnalyzer.Analyze(
            sub,
            TimeSpan.FromMinutes(5),
            currentPrimaryStart,
            TimeSpan.FromHours(1),
            nowInSecondHalf,
            openingBaselineDays: 30,
            intraDayBaselineCandles: 100,
            minSamples: 10,
            triggerThreshold: 2.0m);

        result.Should().BeNull();
    }
}