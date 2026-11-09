using FluentAssertions;
using Trader.Application.Runtime;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Execution;
using Xunit;

namespace Trader.Tests.Infrastructure.Execution;

public class FixedCapitalAllocatorTests
{
    private readonly FixedCapitalAllocator _sut = new();

    [Fact]
    public async Task GetAllocatableCapital_FixedMode_ReturnsFixedAmount()
    {
        var accountId = Guid.NewGuid();
        var ctx = BuildContext(accountId,
            AccountCapitalAllocation.Fixed(new Money(50_000_000m)));

        var capital = await _sut.GetAllocatableCapitalAsync(ctx, accountId);

        capital.Should().Be(50_000_000m);
    }

    [Fact]
    public async Task GetAllocatableCapital_DynamicMode_Throws()
    {
        var accountId = Guid.NewGuid();
        var ctx = BuildContext(accountId,
            AccountCapitalAllocation.Dynamic(50m));

        var act = () => _sut.GetAllocatableCapitalAsync(ctx, accountId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Expected Fixed*");
    }

    [Fact]
    public async Task GetAllocatableCapital_AccountNotInRun_Throws()
    {
        var accountId = Guid.NewGuid();
        var ctx = BuildContext(Guid.NewGuid(),
            AccountCapitalAllocation.Fixed(new Money(100m)));

        var act = () => _sut.GetAllocatableCapitalAsync(ctx, accountId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private static RunRuntimeContext BuildContext(
        Guid accountId, AccountCapitalAllocation allocation)
    {
        return new RunRuntimeContext(
            RunId: Guid.NewGuid(),
            Name: "test",
            OwnerPersonId: Guid.Empty,
            OwnerUserId: Guid.NewGuid(),
            StrategyKey: "rsi",
            Strategy: null!,
            StrategyConfig: null!,
            PrimaryTimeFrame: TimeFrame.D1,
            TargetCurrency: null,
            StrengthMultiplier: 1.0m,
            MinDeltaPercent: 0.03m,
            ExecutionMode: ExecutionMode.Live,
            StopLoss: null,
            TakeProfit: null,
            DefaultOrderMode: OrderMode.Quantity,
            OnMarketClose: MarketCloseBehavior.KeepPositions,
            OnStop: StopRunBehavior.KeepPositions,
            Symbols: new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            AccountAllocations: new[]
            {
                new RunAccountAllocation(accountId, allocation)
            });
    }
}