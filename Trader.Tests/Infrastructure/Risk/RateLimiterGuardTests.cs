using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Risk.Guards;
using Xunit;

namespace Trader.Tests.Infrastructure.Risk;

public class RateLimiterGuardTests
{
    [Fact]
    public async Task Check_Disabled_Allows()
    {
        var sut = CreateSut(enabled: false);
        var decision = await sut.CheckAsync(BuildContext());
        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_PaperMode_DefaultsToSkip()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        var decision = await sut.CheckAsync(BuildContext(ExecutionMode.Paper));
        decision.Kind.Should().Be(RiskDecisionKind.Allow);
        tracker.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Check_UnderLimit_Allows()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        tracker.Setup(x => x.GetCountInWindow(
                It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
            .Returns(5);

        var decision = await sut.CheckAsync(BuildContext(ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_AtLimit_Blocks()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        tracker.Setup(x => x.GetCountInWindow(
                It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
            .Returns(10);   // exactly at limit

        var decision = await sut.CheckAsync(BuildContext(ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Block);
        decision.GuardName.Should().Be("RateLimiter");
        decision.Reason.Should().Contain("Rate limit reached");
        decision.Reason.Should().Contain("10/10");
    }

    [Fact]
    public async Task Check_OverLimit_Blocks()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        tracker.Setup(x => x.GetCountInWindow(
                It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
            .Returns(15);

        var decision = await sut.CheckAsync(BuildContext(ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Block);
    }

    [Fact]
    public async Task Check_PaperModeWithApplyToPaper_Checks()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: true);
        tracker.Setup(x => x.GetCountInWindow(
                It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
            .Returns(10);

        var decision = await sut.CheckAsync(BuildContext(ExecutionMode.Paper));

        decision.Kind.Should().Be(RiskDecisionKind.Block);
    }

    // ─── Helpers ───

    private static RateLimiterGuard CreateSut(bool enabled)
        => CreateSutWithMock(applyToPaper: false, enabled: enabled).Sut;

    private static (RateLimiterGuard Sut, Mock<IRateLimiterTracker> Tracker)
        CreateSutWithMock(bool applyToPaper, bool enabled = true)
    {
        var opts = new Mock<IOptionsMonitor<TradingRiskOptions>>();
        opts.Setup(x => x.CurrentValue).Returns(new TradingRiskOptions
        {
            Enabled = true,
            RateLimiter = new RateLimiterOptions
            {
                Enabled = enabled,
                OrdersPerMinutePerAccount = 10,
                WindowSeconds = 60,
                ApplyToPaper = applyToPaper,
            }
        });

        var tracker = new Mock<IRateLimiterTracker>();
        return (new RateLimiterGuard(tracker.Object, opts.Object), tracker);
    }

    private static RiskCheckContext BuildContext(
        ExecutionMode executionMode = ExecutionMode.Live)
    {
        var account = TraderAccount.Create(BrokerType.EasyTrader, "test", "u", "p");
        var run = TradingRun.Create(
            "test", Guid.NewGuid(), "rsi", "{}", TimeFrame.D1,
            null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions,
            StopRunBehavior.KeepPositions,
            new[] { ("TEST", new SymbolAllocation(100m)) },
            new[]
            {
                (account.Id, AccountCapitalAllocation.Dynamic(100m))
            },
            executionMode: executionMode);

        var ctx = new RunRuntimeContext(
            run.Id, run.Name, Guid.Empty, Guid.NewGuid(),
            "rsi", null!, null!,
            TimeFrame.D1, null, 1.0m, 0.03m, executionMode,
            null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions, StopRunBehavior.KeepPositions,
            new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            new[] { new RunAccountAllocation(account.Id, AccountCapitalAllocation.Dynamic(100m)) });

        var intent = OrderIntent.Create(
            run.Id, account.Id, null, "TEST", OrderSide.Buy,
            new Price(1000m), new Quantity(100), OrderMode.Quantity);

        return new RiskCheckContext(intent, ctx, account);
    }
}