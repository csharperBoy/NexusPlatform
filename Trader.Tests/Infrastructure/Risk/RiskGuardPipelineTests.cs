using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Risk;
using Xunit;

namespace Trader.Tests.Infrastructure.Risk;

public class RiskGuardPipelineTests
{
    [Fact]
    public async Task Check_MasterSwitchOff_AllowsRegardless()
    {
        var sut = CreateSut(
            guards: new[] { new FakeGuard("A", RiskDecisionKind.Block) },
            enabled: false);

        var decision = await sut.CheckAsync(BuildContext());

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_NoGuards_Allows()
    {
        var sut = CreateSut(guards: Array.Empty<IRiskGuard>(), enabled: true);

        var decision = await sut.CheckAsync(BuildContext());

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_AllAllow_ReturnsAllow()
    {
        var sut = CreateSut(new IRiskGuard[]
        {
            new FakeGuard("A", RiskDecisionKind.Allow),
            new FakeGuard("B", RiskDecisionKind.Allow),
        }, enabled: true);

        var decision = await sut.CheckAsync(BuildContext());

        decision.Kind.Should().Be(RiskDecisionKind.Allow);
    }

    [Fact]
    public async Task Check_FirstBlocks_StopsBeforeSecond()
    {
        var a = new FakeGuard("A", RiskDecisionKind.Block);
        var b = new FakeGuard("B", RiskDecisionKind.Allow);

        var sut = CreateSut(new IRiskGuard[] { a, b }, enabled: true);

        var decision = await sut.CheckAsync(BuildContext());

        decision.Kind.Should().Be(RiskDecisionKind.Block);
        decision.GuardName.Should().Be("A");
        a.WasCalled.Should().BeTrue();
        b.WasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Check_GuardThrows_BlocksAsFailSafe()
    {
        var throwing = new ThrowingGuard();
        var sut = CreateSut(new IRiskGuard[] { throwing }, enabled: true);

        var decision = await sut.CheckAsync(BuildContext());

        decision.Kind.Should().Be(RiskDecisionKind.Block);
        decision.Reason.Should().Contain("threw exception");
    }

    // ─── Helpers ───

    private static RiskGuardPipeline CreateSut(
        IEnumerable<IRiskGuard> guards, bool enabled)
    {
        var options = new Mock<IOptionsMonitor<TradingRiskOptions>>();
        options.Setup(x => x.CurrentValue)
            .Returns(new TradingRiskOptions { Enabled = enabled });

        return new RiskGuardPipeline(
            guards,
            options.Object,
            NullLogger<RiskGuardPipeline>.Instance);
    }

    private static RiskCheckContext BuildContext()
    {
        var account = TraderAccount.Create(BrokerType.EasyTrader, "x", "u", "p");
        var run = TradingRun.Create(
            "test", Guid.NewGuid(), "rsi", "{}", TimeFrame.D1,
            CapitalAllocation.Dynamic(), null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions, StopRunBehavior.KeepPositions,
            new[] { ("TEST", new SymbolAllocation(100m)) },
            new[] { account.Id });

        var ctx = new RunRuntimeContext(
            run.Id, run.Name, Guid.Empty, Guid.NewGuid(), "rsi", null!, null!,
            TimeFrame.D1, null, 1.0m, 0.03m, ExecutionMode.Paper,
            run.CapitalAllocation, null, null,
            OrderMode.Quantity, MarketCloseBehavior.KeepPositions, StopRunBehavior.KeepPositions,
            new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            new[] { account.Id });

        var intent = OrderIntent.Create(
            run.Id, account.Id, null, "TEST", OrderSide.Buy,
            new Price(1000m), new Quantity(100), OrderMode.Quantity);

        return new RiskCheckContext(intent, ctx, account);
    }

    private sealed class FakeGuard : IRiskGuard
    {
        private readonly RiskDecisionKind _decision;
        public bool WasCalled { get; private set; }
        public FakeGuard(string name, RiskDecisionKind decision)
        {
            Name = name;
            _decision = decision;
        }
        public string Name { get; }
        public Task<RiskDecision> CheckAsync(RiskCheckContext ctx, CancellationToken ct = default)
        {
            WasCalled = true;
            return Task.FromResult(_decision == RiskDecisionKind.Allow
                ? RiskDecision.Allow
                : RiskDecision.Block("test", Name));
        }
    }

    private sealed class ThrowingGuard : IRiskGuard
    {
        public string Name => "Throwing";
        public Task<RiskDecision> CheckAsync(RiskCheckContext ctx, CancellationToken ct = default)
            => throw new InvalidOperationException("guard fail");
    }
}