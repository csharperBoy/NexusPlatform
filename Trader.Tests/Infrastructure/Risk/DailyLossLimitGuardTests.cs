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

public class DailyLossLimitGuardTests
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
        var sut = CreateSut(enabled: true, applyToPaper: false);
        var tracker = new Mock<IRunDailyEquityTracker>();
        // tracker نباید صدا زده شود

        var decision = await sut.CheckAsync(
            BuildContext(executionMode: ExecutionMode.Paper));

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
        tracker.VerifyNoOtherCalls();
    }

    
    [Fact]
    public async Task Check_PaperModeWithApplyToPaper_BlocksOnExcessLoss()
    {
        var (sut, trackerMock) = CreateSutWithMock(applyToPaper: true);
        trackerMock.Setup(x => x.GetCurrentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCurrent(openingEquity: 100M, currentEquity: 90M));
        // 10% loss > 5% limit

        var decision = await sut.CheckAsync(
            BuildContext(executionMode: ExecutionMode.Paper));

        decision.Kind.Should().Be(RiskDecisionKind.Block);
    }

    [Fact]
    public async Task Check_LossBelowThreshold_Allows()
    {
        var (sut, trackerMock) = CreateSutWithMock(applyToPaper: false);
        trackerMock.Setup(x => x.GetCurrentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCurrent(openingEquity: 100_000_000M, currentEquity: 97_000_000M));
        // 3% loss, limit 5% → Allow

        var decision = await sut.CheckAsync(BuildContext(executionMode: ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_LossAboveThreshold_Blocks()
    {
        var (sut, trackerMock) = CreateSutWithMock(applyToPaper: false);
        trackerMock.Setup(x => x.GetCurrentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCurrent(openingEquity: 100_000_000M, currentEquity: 94_000_000M));
        // 6% loss, limit 5% → Block

        var decision = await sut.CheckAsync(BuildContext(executionMode: ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Block);
        decision.GuardName.Should().Be("DailyLossLimit");
        decision.Reason.Should().Contain("6.00");   // loss percent
        decision.Reason.Should().Contain("5.00");   // limit
    }

    [Fact]
    public async Task Check_EquityIncreased_ShowsZeroLoss()
    {
        var (sut, trackerMock) = CreateSutWithMock(applyToPaper: false);
        trackerMock.Setup(x => x.GetCurrentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCurrent(openingEquity: 100M, currentEquity: 120M));

        var decision = await sut.CheckAsync(BuildContext(executionMode: ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_ZeroOpeningEquity_DoesNotDivideByZero()
    {
        var (sut, trackerMock) = CreateSutWithMock(applyToPaper: false);
        trackerMock.Setup(x => x.GetCurrentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCurrent(openingEquity: 0M, currentEquity: 0M));

        var decision = await sut.CheckAsync(BuildContext(executionMode: ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_TrackerThrows_Propagates()
    {
        var (sut, trackerMock) = CreateSutWithMock(applyToPaper: false);
        trackerMock.Setup(x => x.GetCurrentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("broker down"));

        var act = () => sut.CheckAsync(BuildContext(executionMode: ExecutionMode.Live));

        await act.Should().ThrowAsync<InvalidOperationException>();
        // pipeline این را به Block تبدیل می‌کند (fail-safe)
    }

    // ─── Helpers ───

    private static DailyLossLimitGuard CreateSut(bool enabled, bool applyToPaper = false)
        => CreateSutWithMock(applyToPaper, enabled).Sut;

    private static (DailyLossLimitGuard Sut, Mock<IRunDailyEquityTracker> Tracker) CreateSutWithMock(
        bool applyToPaper, bool enabled = true, decimal maxLoss = 5.0m)
    {
        var opts = new Mock<IOptionsMonitor<TradingRiskOptions>>();
        opts.Setup(x => x.CurrentValue).Returns(new TradingRiskOptions
        {
            Enabled = true,
            DailyLossLimit = new DailyLossLimitOptions
            {
                Enabled = enabled,
                MaxLossPercent = maxLoss,
                ApplyToPaper = applyToPaper,
            }
        });

        var tracker = new Mock<IRunDailyEquityTracker>();
        var sut = new DailyLossLimitGuard(tracker.Object, opts.Object);
        return (sut, tracker);
    }

    private static RunDailyEquityCurrent BuildCurrent(
        decimal openingEquity, decimal currentEquity)
    {
        // Opening equity = openingCash + openingPositionValue
        var opening = new RunDailyEquityOpening(
            RunId: Guid.NewGuid(),
            TradingDate: new DateOnly(2026, 11, 8),
            OpeningCash: openingEquity,
            OpeningPositionValue: 0m,
            OpeningEquity: openingEquity,
            CapturedAt: DateTime.UtcNow);

        return new RunDailyEquityCurrent(
            Opening: opening,
            CurrentCash: currentEquity,
            CurrentPositionValue: 0m,
            LastCashRefreshAt: DateTime.UtcNow);
    }

    private static RiskCheckContext BuildContext(
        ExecutionMode executionMode = ExecutionMode.Live)
    {
        var account = TraderAccount.Create(BrokerType.EasyTrader, "test", "u", "p");

        var run = TradingRun.Create(
            "test", Guid.NewGuid(), "rsi", "{}", TimeFrame.D1,
            CapitalAllocation.Dynamic(), null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions,
            StopRunBehavior.KeepPositions,
            new[] { ("TEST", new SymbolAllocation(100m)) },
            new[] { account.Id },
            executionMode: executionMode);

        var ctx = new RunRuntimeContext(
            run.Id, run.Name, Guid.Empty, Guid.NewGuid(),
            "rsi", null!, null!,
            TimeFrame.D1, null, 1.0m, 0.03m,
            executionMode,
            run.CapitalAllocation, null, null,
            OrderMode.Quantity,
            MarketCloseBehavior.KeepPositions,
            StopRunBehavior.KeepPositions,
            new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            new[] { account.Id });

        var intent = OrderIntent.Create(
            run.Id, account.Id, null, "TEST", OrderSide.Buy,
            new Price(1000m), new Quantity(100), OrderMode.Quantity);

        return new RiskCheckContext(intent, ctx, account);
    }
}