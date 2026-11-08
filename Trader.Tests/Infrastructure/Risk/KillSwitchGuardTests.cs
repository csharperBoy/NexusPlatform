using FluentAssertions;
using Moq;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Risk.Guards;
using Xunit;

namespace Trader.Tests.Infrastructure.Risk;

public class KillSwitchGuardTests
{
    [Fact]
    public async Task Check_NoActiveKillSwitch_Allows()
    {
        var tracker = new Mock<IRiskStateTracker>();
        tracker.SetupGet(x => x.IsGlobalKillSwitchActive).Returns(false);
        tracker.Setup(x => x.IsAccountKillSwitchActive(It.IsAny<Guid>())).Returns(false);

        var sut = new KillSwitchGuard(tracker.Object);

        var decision = await sut.CheckAsync(BuildContext());

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_GlobalActive_Blocks()
    {
        var tracker = new Mock<IRiskStateTracker>();
        tracker.SetupGet(x => x.IsGlobalKillSwitchActive).Returns(true);

        var sut = new KillSwitchGuard(tracker.Object);

        var decision = await sut.CheckAsync(BuildContext());

        decision.Kind.Should().Be(RiskDecisionKind.Block);
        decision.GuardName.Should().Be("KillSwitch");
        decision.Reason.Should().Contain("Global");
    }

    [Fact]
    public async Task Check_AccountActive_Blocks()
    {
        var tracker = new Mock<IRiskStateTracker>();
        tracker.SetupGet(x => x.IsGlobalKillSwitchActive).Returns(false);

        // AccountId از خود Context می‌آید — Mock نمی‌توانیم قبل از ساخت بدانیم
        var sut = new KillSwitchGuard(tracker.Object);
        var context = BuildContext();
        var accountId = context.Account.Id;

        tracker.Setup(x => x.IsAccountKillSwitchActive(accountId)).Returns(true);

        var decision = await sut.CheckAsync(context);

        decision.Kind.Should().Be(RiskDecisionKind.Block);
    }

    [Fact]
    public async Task Check_DifferentAccountActive_Allows()
    {
        var activeAccount = Guid.NewGuid();
        var contextAccount = Guid.NewGuid();

        var tracker = new Mock<IRiskStateTracker>();
        tracker.SetupGet(x => x.IsGlobalKillSwitchActive).Returns(false);
        tracker.Setup(x => x.IsAccountKillSwitchActive(activeAccount)).Returns(true);
        tracker.Setup(x => x.IsAccountKillSwitchActive(contextAccount)).Returns(false);

        var sut = new KillSwitchGuard(tracker.Object);

        var decision = await sut.CheckAsync(BuildContext(contextAccount));

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    // ─── Helpers ───

    private static RiskCheckContext BuildContext(Guid? accountId = null)
    {
        var account = TraderAccount.Create(
            BrokerType.EasyTrader,
            "test account",
            "user",
            "enc-password");

        // account.Id از BaseEntity می‌آید — برای تست، Id را override نمی‌توانیم.
        // به‌جای آن، از account واقعی استفاده می‌کنیم.
        // اگر accountId سفارشی داده شد، این مسئله را باید با Mock حل کنیم.

        var run = TradingRun.Create(
            name: "test",
            ownerPersonId: Guid.NewGuid(),
            strategyKey: "rsi",
            strategyConfigJson: "{}",
            timeFrame: TimeFrame.D1,
            capitalAllocation: CapitalAllocation.Dynamic(),
            stopLoss: null,
            takeProfit: null,
            defaultOrderMode: OrderMode.Quantity,
            onMarketClose: MarketCloseBehavior.KeepPositions,
            onStop: StopRunBehavior.KeepPositions,
            symbols: new[] { ("TEST", new SymbolAllocation(100m)) },
            accountIds: new[] { account.Id });

        var runContext = new RunRuntimeContext(
            RunId: run.Id,
            Name: run.Name,
            OwnerPersonId: run.OwnerPersonId ?? Guid.Empty,
            OwnerUserId: Guid.NewGuid(),
            StrategyKey: run.StrategyKey,
            Strategy: null!,   // در این تست نیازی نیست
            StrategyConfig: null!,
            PrimaryTimeFrame: run.PrimaryTimeFrame,
            TargetCurrency: null,
            StrengthMultiplier: 1.0m,
            MinDeltaPercent: 0.03m,
            ExecutionMode: ExecutionMode.Paper,
            CapitalAllocation: run.CapitalAllocation,
            StopLoss: null,
            TakeProfit: null,
            DefaultOrderMode: OrderMode.Quantity,
            OnMarketClose: MarketCloseBehavior.KeepPositions,
            OnStop: StopRunBehavior.KeepPositions,
            Symbols: new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            AccountIds: new[] { account.Id });

        var intent = OrderIntent.Create(
            runId: run.Id,
            traderAccountId: account.Id,
            sourceSignalId: null,
            isin: "TEST",
            side: OrderSide.Buy,
            price: new Price(1000m),
            quantity: new Quantity(100),
            orderMode: OrderMode.Quantity);

        return new RiskCheckContext(intent, runContext, account);
    }
}