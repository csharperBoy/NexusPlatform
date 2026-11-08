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
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        var decision = await sut.CheckAsync(
            BuildContext(executionMode: ExecutionMode.Paper));
        decision.Kind.Should().Be(RiskDecisionKind.Allow);
        tracker.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Check_LossBelowThreshold_Allows()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        tracker.Setup(x => x.GetCurrentAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCurrent(openingEquity: 100_000_000M, currentEquity: 97_000_000M));

        var decision = await sut.CheckAsync(BuildContext(executionMode: ExecutionMode.Live));
        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_LossAboveThreshold_Blocks()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        tracker.Setup(x => x.GetCurrentAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildCurrent(openingEquity: 100_000_000M, currentEquity: 94_000_000M));

        var decision = await sut.CheckAsync(BuildContext(executionMode: ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Block);
        decision.GuardName.Should().Be("DailyLossLimit");
        decision.Reason.Should().Contain("6.00");
        decision.Reason.Should().Contain("5.00");
    }

    [Fact]
    public async Task Check_PerAccount_Isolation()
    {
        var accountA = TraderAccount.Create(BrokerType.EasyTrader, "A", "u", "p");
        var accountB = TraderAccount.Create(BrokerType.EasyTrader, "B", "u2", "p2");

        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        tracker.Setup(x => x.GetCurrentAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid runId, Guid accId, CancellationToken ct) =>
                accId == accountA.Id
                    ? BuildCurrent(openingEquity: 100M, currentEquity: 90M)   // 10% loss
                    : BuildCurrent(openingEquity: 100M, currentEquity: 99M)); // 1% loss

        var decisionA = await sut.CheckAsync(BuildContextFor(accountA));
        var decisionB = await sut.CheckAsync(BuildContextFor(accountB));

        decisionA.Kind.Should().Be(RiskDecisionKind.Block);
        decisionB.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    // ─── Helpers ───
    private static RiskCheckContext BuildContextFor(
    TraderAccount account, ExecutionMode executionMode = ExecutionMode.Live)
    {
        var run = TradingRun.Create(
            "test", Guid.NewGuid(), "rsi", "{}", TimeFrame.D1,
            CapitalAllocation.Dynamic(), null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions,
            StopRunBehavior.KeepPositions,
            new[] { ("TEST", new SymbolAllocation(100m)) },
            new[] { (account.Id, 100m) },
            executionMode: executionMode);

        var ctx = new RunRuntimeContext(
            run.Id, run.Name, Guid.Empty, Guid.NewGuid(),
            "rsi", null!, null!, TimeFrame.D1, null, 1.0m, 0.03m, executionMode,
            run.CapitalAllocation, null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions, StopRunBehavior.KeepPositions,
            new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            new[] { new RunAccountAllocation(account.Id, 100m) });

        var intent = OrderIntent.Create(
            run.Id, account.Id, null, "TEST", OrderSide.Buy,
            new Price(1000m), new Quantity(100), OrderMode.Quantity);

        return new RiskCheckContext(intent, ctx, account);
    }
    private static DailyLossLimitGuard CreateSut(bool enabled)
        => CreateSutWithMock(applyToPaper: false, enabled: enabled).Sut;

    private static (DailyLossLimitGuard Sut, Mock<IRunDailyEquityTracker> Tracker)
        CreateSutWithMock(bool applyToPaper, bool enabled = true)
    {
        var opts = new Mock<IOptionsMonitor<TradingRiskOptions>>();
        opts.Setup(x => x.CurrentValue).Returns(new TradingRiskOptions
        {
            Enabled = true,
            DailyLossLimit = new DailyLossLimitOptions
            {
                Enabled = enabled,
                MaxLossPercent = 5.0m,
                ApplyToPaper = applyToPaper,
            }
        });
        var tracker = new Mock<IRunDailyEquityTracker>();
        return (new DailyLossLimitGuard(tracker.Object, opts.Object), tracker);
    }

    private static RunAccountDailyEquityCurrent BuildCurrent(
        decimal openingEquity, decimal currentEquity)
    {
        var opening = new RunAccountDailyEquityOpening(
            RunId: Guid.NewGuid(),
            AccountId: Guid.NewGuid(),
            TradingDate: new DateOnly(2026, 11, 8),
            OpeningCash: openingEquity,
            OpeningPositionValue: 0m,
            OpeningEquity: openingEquity,
            CapturedAt: DateTime.UtcNow);

        return new RunAccountDailyEquityCurrent(
            Opening: opening,
            CurrentCash: currentEquity,
            CurrentPositionValue: 0m,
            LastCashRefreshAt: DateTime.UtcNow);
    }

    private static RiskCheckContext BuildContext(
        ExecutionMode executionMode = ExecutionMode.Live)
        => BuildContextForAccount(Guid.NewGuid(), executionMode);

    private static RiskCheckContext BuildContextForAccount(
        Guid accountId, ExecutionMode executionMode = ExecutionMode.Live)
    {
        var account = TraderAccount.Create(BrokerType.EasyTrader, "test", "u", "p");
        // account.Id از BaseEntity — نمی‌توان override کرد
        // برای تست per-account، از account.Id واقعی استفاده می‌کنیم

        var run = TradingRun.Create(
            "test", Guid.NewGuid(), "rsi", "{}", TimeFrame.D1,
            CapitalAllocation.Dynamic(), null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions,
            StopRunBehavior.KeepPositions,
            new[] { ("TEST", new SymbolAllocation(100m)) },
            new[] { (account.Id, 100m) },   // ← changed
            executionMode: executionMode);

        var accountAllocs = new[]
        {
            new RunAccountAllocation(account.Id, 100m)
        };

        var ctx = new RunRuntimeContext(
            run.Id, run.Name, Guid.Empty, Guid.NewGuid(),
            "rsi", null!, null!,
            TimeFrame.D1, null, 1.0m, 0.03m, executionMode,
            run.CapitalAllocation, null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions, StopRunBehavior.KeepPositions,
            new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            accountAllocs);   // ← changed

        var intent = OrderIntent.Create(
            run.Id, account.Id, null, "TEST", OrderSide.Buy,
            new Price(1000m), new Quantity(100), OrderMode.Quantity);

        return new RiskCheckContext(intent, ctx, account);
    }
}