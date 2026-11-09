using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Trader.Application.Abstractions;
using Trader.Application.Runtime;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Execution;
using Xunit;

namespace Trader.Tests.Infrastructure.Execution;

public class CompositeCapitalAllocatorTests
{
    [Fact]
    public async Task GetAllocatableCapital_FixedMode_DelegatesToFixed()
    {
        var accountId = Guid.NewGuid();
        var (sut, _) = CreateSut();
        var ctx = BuildContextWithAccount(accountId,
            AccountCapitalAllocation.Fixed(new Money(75_000_000m)));

        var capital = await sut.GetAllocatableCapitalAsync(ctx, accountId);

        capital.Should().Be(75_000_000m);
    }

    [Fact]
    public async Task GetAllocatableCapital_DynamicMode_DelegatesToDynamic()
    {
        var accountId = Guid.NewGuid();
        var (sut, cashCache) = CreateSut();
        cashCache.Setup(x => x.GetOrRefreshAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountCashSnapshot(accountId, 200_000_000m, DateTime.UtcNow, "Broker"));

        var ctx = BuildContextWithAccount(accountId,
            AccountCapitalAllocation.Dynamic(40m));

        var capital = await sut.GetAllocatableCapitalAsync(ctx, accountId);

        capital.Should().Be(80_000_000m);   // 200M × 40%
    }

    [Fact]
    public async Task GetAllocatableCapital_TwoAccounts_DifferentModes()
    {
        var fixedAccount = Guid.NewGuid();
        var dynamicAccount = Guid.NewGuid();

        var (sut, cashCache) = CreateSut();
        cashCache.Setup(x => x.GetOrRefreshAsync(dynamicAccount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountCashSnapshot(dynamicAccount, 100_000_000m, DateTime.UtcNow, "Broker"));

        var ctx = BuildContextWithTwoAccounts(
            fixedAccount, AccountCapitalAllocation.Fixed(new Money(30_000_000m)),
            dynamicAccount, AccountCapitalAllocation.Dynamic(50m));

        var fixedCapital = await sut.GetAllocatableCapitalAsync(ctx, fixedAccount);
        var dynamicCapital = await sut.GetAllocatableCapitalAsync(ctx, dynamicAccount);

        fixedCapital.Should().Be(30_000_000m);
        dynamicCapital.Should().Be(50_000_000m);   // 100M × 50%
    }

    private static (CompositeCapitalAllocator Sut, Mock<IAccountCashCache> CashCache) CreateSut()
    {
        var cashCache = new Mock<IAccountCashCache>();
        var dynamicAlloc = new DynamicCapitalAllocator(
            cashCache.Object, NullLogger<DynamicCapitalAllocator>.Instance);
        var fixedAlloc = new FixedCapitalAllocator();
        var sut = new CompositeCapitalAllocator(fixedAlloc, dynamicAlloc);
        return (sut, cashCache);
    }

    private static RunRuntimeContext BuildContextWithAccount(
        Guid accountId, AccountCapitalAllocation allocation)
        => BuildContextWithTwoAccounts(accountId, allocation, Guid.NewGuid(),
            AccountCapitalAllocation.Dynamic(100m));

    private static RunRuntimeContext BuildContextWithTwoAccounts(
        Guid aId, AccountCapitalAllocation aAlloc,
        Guid bId, AccountCapitalAllocation bAlloc)
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
                new RunAccountAllocation(aId, aAlloc),
                new RunAccountAllocation(bId, bAlloc),
            });
    }
}