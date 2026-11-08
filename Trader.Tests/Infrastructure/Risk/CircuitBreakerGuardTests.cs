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

public class CircuitBreakerGuardTests
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
    public async Task Check_CircuitActive_Blocks()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        var expires = DateTime.UtcNow.AddMinutes(30);
        tracker.Setup(x => x.GetAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LossStreakState(
                Guid.NewGuid(), Guid.NewGuid(), 5,
                DateTime.UtcNow.AddMinutes(-5), null,
                DateTime.UtcNow.AddMinutes(-5), expires));

        var decision = await sut.CheckAsync(BuildContext(ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Block);
        decision.GuardName.Should().Be("CircuitBreaker");
        decision.Reason.Should().Contain("Circuit breaker active");
    }

    [Fact]
    public async Task Check_CircuitNotActive_Allows()
    {
        var (sut, tracker) = CreateSutWithMock(applyToPaper: false);
        tracker.Setup(x => x.GetAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LossStreakState(
                Guid.NewGuid(), Guid.NewGuid(), 2,
                DateTime.UtcNow.AddMinutes(-5), null,
                null, null));   // no circuit

        var decision = await sut.CheckAsync(BuildContext(ExecutionMode.Live));

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    // ─── Helpers ───

    private static CircuitBreakerGuard CreateSut(bool enabled)
        => CreateSutWithMock(applyToPaper: false, enabled: enabled).Sut;

    private static (CircuitBreakerGuard Sut, Mock<IRunAccountLossStreakTracker> Tracker)
        CreateSutWithMock(bool applyToPaper, bool enabled = true)
    {
        var opts = new Mock<IOptionsMonitor<TradingRiskOptions>>();
        opts.Setup(x => x.CurrentValue).Returns(new TradingRiskOptions
        {
            Enabled = true,
            CircuitBreaker = new CircuitBreakerOptions
            {
                Enabled = enabled,
                ConsecutiveLosses = 5,
                CooldownMinutes = 60,
                ApplyToPaper = applyToPaper,
            }
        });
        var tracker = new Mock<IRunAccountLossStreakTracker>();
        return (new CircuitBreakerGuard(tracker.Object, opts.Object), tracker);
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
            new[] { (account.Id, 100m) },
            executionMode: executionMode);

        var ctx = new RunRuntimeContext(
            run.Id, run.Name, Guid.Empty, Guid.NewGuid(), "rsi", null!, null!,
            TimeFrame.D1, null, 1.0m, 0.03m, executionMode,
            run.CapitalAllocation, null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions, StopRunBehavior.KeepPositions,
            new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            new[] { new RunAccountAllocation(account.Id, 100m) });

        var intent = OrderIntent.Create(
            run.Id, account.Id, null, "TEST", OrderSide.Buy,
            new Price(1000m), new Quantity(100), OrderMode.Quantity);

        return new RiskCheckContext(intent, ctx, account);
    }
}