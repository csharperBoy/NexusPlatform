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

public class DynamicCapitalAllocatorTests
{
    [Fact]
    public async Task GetAllocatableCapital_100Percent_ReturnsFullBuyPower()
    {
        var accountId = Guid.NewGuid();
        var (sut, cashCache) = CreateSut();
        cashCache.Setup(x => x.GetOrRefreshAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountCashSnapshot(accountId, 200_000_000m, DateTime.UtcNow, "Broker"));

        var ctx = BuildContext(accountId, 100m);

        var capital = await sut.GetAllocatableCapitalAsync(ctx, accountId);

        capital.Should().Be(200_000_000m);
    }

    [Fact]
    public async Task GetAllocatableCapital_50Percent_ReturnsHalfBuyPower()
    {
        var accountId = Guid.NewGuid();
        var (sut, cashCache) = CreateSut();
        cashCache.Setup(x => x.GetOrRefreshAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountCashSnapshot(accountId, 200_000_000m, DateTime.UtcNow, "Broker"));

        var ctx = BuildContext(accountId, 50m);

        var capital = await sut.GetAllocatableCapitalAsync(ctx, accountId);

        capital.Should().Be(100_000_000m);
    }

    [Fact]
    public async Task GetAllocatableCapital_AccountNotInRun_Throws()
    {
        var runAccountId = Guid.NewGuid();
        var unknownAccountId = Guid.NewGuid();

        var (sut, cashCache) = CreateSut();
        cashCache.Setup(x => x.GetOrRefreshAsync(unknownAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountCashSnapshot(unknownAccountId, 100m, DateTime.UtcNow, "Broker"));

        var ctx = BuildContext(runAccountId, 100m);   // Context فقط runAccountId را دارد

        var act = () => sut.GetAllocatableCapitalAsync(ctx, unknownAccountId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── Helpers ───

    private static (DynamicCapitalAllocator Sut, Mock<IAccountCashCache> CashCache) CreateSut()
    {
        var cashCache = new Mock<IAccountCashCache>();
        var sut = new DynamicCapitalAllocator(
            cashCache.Object, NullLogger<DynamicCapitalAllocator>.Instance);
        return (sut, cashCache);
    }

    private static RunRuntimeContext BuildContext(Guid accountId, decimal percent)
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
            CapitalAllocation: CapitalAllocation.Dynamic(),
            StopLoss: null,
            TakeProfit: null,
            DefaultOrderMode: OrderMode.Quantity,
            OnMarketClose: MarketCloseBehavior.KeepPositions,
            OnStop: StopRunBehavior.KeepPositions,
            Symbols: new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
            AccountAllocations: new[] { new RunAccountAllocation(accountId, percent) });
    }
}