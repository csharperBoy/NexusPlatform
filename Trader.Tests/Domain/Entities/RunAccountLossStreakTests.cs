using FluentAssertions;
using Trader.Domain.Entities;
using Xunit;

namespace Trader.Tests.Domain.Entities;

public class RunAccountLossStreakTests
{
    [Fact]
    public void Create_InitialState_ZeroLossesNoCircuit()
    {
        var s = RunAccountLossStreak.Create(Guid.NewGuid(), Guid.NewGuid());
        s.ConsecutiveLosses.Should().Be(0);
        s.CircuitBrokenAt.Should().BeNull();
        s.CooldownExpiresAt.Should().BeNull();
    }

    [Fact]
    public void RecordLoss_BelowThreshold_IncrementsCounterOnly()
    {
        var s = RunAccountLossStreak.Create(Guid.NewGuid(), Guid.NewGuid());

        s.RecordLoss(DateTime.UtcNow, threshold: 5, TimeSpan.FromMinutes(60));
        s.RecordLoss(DateTime.UtcNow, threshold: 5, TimeSpan.FromMinutes(60));

        s.ConsecutiveLosses.Should().Be(2);
        s.CircuitBrokenAt.Should().BeNull();
    }

    [Fact]
    public void RecordLoss_ReachesThreshold_TriggersCircuit()
    {
        var s = RunAccountLossStreak.Create(Guid.NewGuid(), Guid.NewGuid());
        var now = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
            s.RecordLoss(now, threshold: 5, TimeSpan.FromMinutes(60));

        s.ConsecutiveLosses.Should().Be(5);
        s.CircuitBrokenAt.Should().Be(now);
        s.CooldownExpiresAt.Should().Be(now.AddMinutes(60));
    }

    [Fact]
    public void RecordWin_ResetsCounterAndClearsCircuit()
    {
        var s = RunAccountLossStreak.Create(Guid.NewGuid(), Guid.NewGuid());
        var now = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
            s.RecordLoss(now, threshold: 5, TimeSpan.FromMinutes(60));

        s.RecordWin(now.AddMinutes(5));

        s.ConsecutiveLosses.Should().Be(0);
        s.CircuitBrokenAt.Should().BeNull();
        s.CooldownExpiresAt.Should().BeNull();
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var s = RunAccountLossStreak.Create(Guid.NewGuid(), Guid.NewGuid());
        s.RecordLoss(DateTime.UtcNow, threshold: 5, TimeSpan.FromMinutes(60));

        s.Reset();

        s.ConsecutiveLosses.Should().Be(0);
        s.CircuitBrokenAt.Should().BeNull();
    }
}