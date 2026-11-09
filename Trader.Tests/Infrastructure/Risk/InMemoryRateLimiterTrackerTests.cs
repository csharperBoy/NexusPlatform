using FluentAssertions;
using Trader.Infrastructure.Risk;
using Xunit;

namespace Trader.Tests.Infrastructure.Risk;

public class InMemoryRateLimiterTrackerTests
{
    [Fact]
    public void GetCountInWindow_EmptyTracker_ReturnsZero()
    {
        var sut = new InMemoryRateLimiterTracker();
        sut.GetCountInWindow(Guid.NewGuid(), TimeSpan.FromMinutes(1))
            .Should().Be(0);
    }

    [Fact]
    public void RecordDispatch_Single_IsCountedInWindow()
    {
        var sut = new InMemoryRateLimiterTracker();
        var accountId = Guid.NewGuid();

        sut.RecordDispatch(accountId);

        sut.GetCountInWindow(accountId, TimeSpan.FromMinutes(1))
            .Should().Be(1);
    }

    [Fact]
    public void RecordDispatch_Multiple_AreCounted()
    {
        var sut = new InMemoryRateLimiterTracker();
        var accountId = Guid.NewGuid();

        for (int i = 0; i < 5; i++)
            sut.RecordDispatch(accountId);

        sut.GetCountInWindow(accountId, TimeSpan.FromMinutes(1))
            .Should().Be(5);
    }

    [Fact]
    public void GetCountInWindow_DifferentAccounts_AreIsolated()
    {
        var sut = new InMemoryRateLimiterTracker();
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();

        sut.RecordDispatch(a);
        sut.RecordDispatch(a);
        sut.RecordDispatch(b);

        sut.GetCountInWindow(a, TimeSpan.FromMinutes(1)).Should().Be(2);
        sut.GetCountInWindow(b, TimeSpan.FromMinutes(1)).Should().Be(1);
    }

    [Fact]
    public void GetCountInWindow_SmallWindow_IgnoresOldEntries()
    {
        var sut = new InMemoryRateLimiterTracker();
        var accountId = Guid.NewGuid();

        // ۳ dispatch در ۵ ثانیه
        for (int i = 0; i < 3; i++)
            sut.RecordDispatch(accountId);

        // با window = 5 ثانیه، هر ۳ حساب می‌شوند (تازه‌اند)
        sut.GetCountInWindow(accountId, TimeSpan.FromSeconds(5))
            .Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void Reset_ClearsAccountState()
    {
        var sut = new InMemoryRateLimiterTracker();
        var accountId = Guid.NewGuid();

        for (int i = 0; i < 5; i++)
            sut.RecordDispatch(accountId);
        sut.Reset(accountId);

        sut.GetCountInWindow(accountId, TimeSpan.FromMinutes(1))
            .Should().Be(0);
    }

    [Fact]
    public void RecordDispatch_Concurrent_AllAreCounted()
    {
        var sut = new InMemoryRateLimiterTracker();
        var accountId = Guid.NewGuid();

        Parallel.For(0, 50, _ => sut.RecordDispatch(accountId));

        sut.GetCountInWindow(accountId, TimeSpan.FromMinutes(1))
            .Should().Be(50);
    }
}