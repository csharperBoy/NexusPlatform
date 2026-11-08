using Core.Application.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.Positions;
using Xunit;

namespace Trader.Tests.Infrastructure.Positions;

public class PositionManagerTests
{
    // ═══════════════════════════════════════════════════════════════
    // Buy — New Position
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ApplyTradeAsync_BuyNew_OpensPosition()
    {
        // Arrange
        var (sut, deps) = CreateSut();
        var order = CreateOrder(OrderSide.Buy);
        var trade = CreateTrade(order, OrderSide.Buy, qty: 100, price: 1000m);

        Position? added = null;
        deps.Positions.Setup(x => x.AddAsync(It.IsAny<Position>()))
            .Callback<Position>(p => added = p)
            .Returns(Task.CompletedTask);

        // Act
        await sut.ApplyTradeAsync(order, trade);

        // Assert
        added.Should().NotBeNull();
        added!.RunId.Should().Be(order.RunId);
        added.TraderAccountId.Should().Be(order.TraderAccountId);
        added.Isin.Should().Be("TEST");
        added.Quantity.Value.Should().Be(100);
        added.AverageEntryPrice.Value.Should().Be(1000m);
        added.State.Should().Be(PositionState.Open);
    }

    // ═══════════════════════════════════════════════════════════════
    // Buy — Increase Existing Position
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ApplyTradeAsync_BuyExisting_IncreasesQuantity()
    {
        // Arrange
        var (sut, deps) = CreateSut();
        var order = CreateOrder(OrderSide.Buy);

        var existing = Position.Open(
            runId: order.RunId,
            traderAccountId: order.TraderAccountId,
            isin: order.Isin,
            quantity: new Quantity(100),
            entryPrice: new Price(1000m),
            stopLossPrice: null,
            takeProfitPrice: null);

        deps.PositionSpecs.Setup(x => x.ListBySpecAsync(
                It.IsAny<Core.Domain.Specifications.ISpecification<Position>>()))
            .ReturnsAsync(new[] { existing });

        var trade = CreateTrade(order, OrderSide.Buy, qty: 100, price: 2000m);

        // Act
        await sut.ApplyTradeAsync(order, trade);

        // Assert
        existing.Quantity.Value.Should().Be(200);
        existing.AverageEntryPrice.Value.Should().Be(1500m);  // (100×1000 + 100×2000) / 200
    }

    // ═══════════════════════════════════════════════════════════════
    // Sell — Partial Close
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ApplyTradeAsync_SellPartial_ReducesQuantity()
    {
        // Arrange
        var (sut, deps) = CreateSut();
        var order = CreateOrder(OrderSide.Sell);

        var existing = Position.Open(
            runId: order.RunId,
            traderAccountId: order.TraderAccountId,
            isin: order.Isin,
            quantity: new Quantity(100),
            entryPrice: new Price(1000m),
            stopLossPrice: null,
            takeProfitPrice: null);

        deps.PositionSpecs.Setup(x => x.ListBySpecAsync(
                It.IsAny<Core.Domain.Specifications.ISpecification<Position>>()))
            .ReturnsAsync(new[] { existing });

        var trade = CreateTrade(order, OrderSide.Sell, qty: 40, price: 1500m);

        // Act
        await sut.ApplyTradeAsync(order, trade);

        // Assert
        existing.Quantity.Value.Should().Be(60);
        existing.RealizedPnL.Amount.Should().Be(20_000m);  // 40 × (1500 - 1000)
        existing.State.Should().Be(PositionState.PartiallyClosed);
    }

    // ═══════════════════════════════════════════════════════════════
    // Sell — Full Close
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ApplyTradeAsync_SellFull_ClosesPosition()
    {
        // Arrange
        var (sut, deps) = CreateSut();
        var order = CreateOrder(OrderSide.Sell);

        var existing = Position.Open(
            runId: order.RunId,
            traderAccountId: order.TraderAccountId,
            isin: order.Isin,
            quantity: new Quantity(100),
            entryPrice: new Price(1000m),
            stopLossPrice: null,
            takeProfitPrice: null);

        deps.PositionSpecs.Setup(x => x.ListBySpecAsync(
                It.IsAny<Core.Domain.Specifications.ISpecification<Position>>()))
            .ReturnsAsync(new[] { existing });

        var trade = CreateTrade(order, OrderSide.Sell, qty: 100, price: 1100m);

        // Act
        await sut.ApplyTradeAsync(order, trade);

        // Assert
        existing.Quantity.Value.Should().Be(0);
        existing.State.Should().Be(PositionState.Closed);
        existing.ClosedAt.Should().NotBeNull();
        existing.RealizedPnL.Amount.Should().Be(10_000m);  // 100 × (1100 - 1000)
    }

    // ═══════════════════════════════════════════════════════════════
    // Sell — Overshoot → Run Faulted (C4)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ApplyTradeAsync_SellOvershoot_FaultsRun_DoesNotApplyFill()
    {
        // Arrange
        var (sut, deps) = CreateSut();
        var order = CreateOrder(OrderSide.Sell);

        // Existing position: 100 shares tracked, but broker sends sell of 150
        var existing = Position.Open(
            runId: order.RunId,
            traderAccountId: order.TraderAccountId,
            isin: order.Isin,
            quantity: new Quantity(100),
            entryPrice: new Price(1000m),
            stopLossPrice: null,
            takeProfitPrice: null);

        deps.PositionSpecs.Setup(x => x.ListBySpecAsync(
                It.IsAny<Core.Domain.Specifications.ISpecification<Position>>()))
            .ReturnsAsync(new[] { existing });

        // Run must be in Running state to be faultable
        var run = CreateRunningRun(order.TraderAccountId);
        deps.TradingRuns.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(run);
        deps.TradingRuns.Setup(x => x.UpdateAsync(It.IsAny<TradingRun>()))
            .Returns(Task.CompletedTask);

        var trade = CreateTrade(order, OrderSide.Sell, qty: 150, price: 1000m);

        // Act
        await sut.ApplyTradeAsync(order, trade);

        // ═══ Assert 1: Position NOT mutated ═══
        existing.Quantity.Value.Should().Be(100,
            because: "on overshoot, the fill must NOT be applied");
        existing.State.Should().Be(PositionState.Open,
            because: "position must stay in its known-good state");

        // ═══ Assert 2: Run marked as Faulted ═══
        run.State.Should().Be(TradingRunState.Faulted);

        // ═══ Assert 3: Critical event logged with PositionOvershootDetected ═══
        deps.EventLogger.Verify(x => x.Log(
            TradingStage.OrderLifecycle,
            TradingEventType.PositionOvershootDetected,
            TradingEventSeverity.Critical,
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<Guid?>(),
            It.IsAny<string?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>()),
            Times.Once);

        // ═══ Assert 4: Run removed from runtime cache ═══
        deps.RunCache.Verify(x => x.Remove(order.RunId), Times.Once);
    }

    [Fact]
    public async Task ApplyTradeAsync_SellOvershoot_DoesNotEmitInfoEvents()
    {
        // Arrange
        var (sut, deps) = CreateSut();
        var order = CreateOrder(OrderSide.Sell);

        var existing = Position.Open(
            runId: order.RunId,
            traderAccountId: order.TraderAccountId,
            isin: order.Isin,
            quantity: new Quantity(100),
            entryPrice: new Price(1000m),
            stopLossPrice: null,
            takeProfitPrice: null);

        deps.PositionSpecs.Setup(x => x.ListBySpecAsync(
                It.IsAny<Core.Domain.Specifications.ISpecification<Position>>()))
            .ReturnsAsync(new[] { existing });

        var run = CreateRunningRun(order.TraderAccountId);
        deps.TradingRuns.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(run);
        deps.TradingRuns.Setup(x => x.UpdateAsync(It.IsAny<TradingRun>()))
            .Returns(Task.CompletedTask);

        var trade = CreateTrade(order, OrderSide.Sell, qty: 150, price: 1000m);

        // Act
        await sut.ApplyTradeAsync(order, trade);

        // Assert: هیچ PositionReduced/PositionClosed با Severity Info نباید صادر شود
        deps.EventLogger.Verify(x => x.Log(
            It.IsAny<TradingStage>(),
            It.IsIn(TradingEventType.PositionReduced, TradingEventType.PositionClosed),
            TradingEventSeverity.Info,
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<Guid?>(),
            It.IsAny<string?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Edge Case — Zero Quantity Trade
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ApplyTradeAsync_ZeroQuantity_NoOp()
    {
        // Arrange
        var (sut, deps) = CreateSut();
        var order = CreateOrder(OrderSide.Buy);

        var trade = OrderTrade.Create(
            brokerOrderId: order.Id,
            tradeNumber: 1,
            isin: order.Isin,
            side: OrderSide.Buy,
            price: new Price(1000m),
            quantity: Quantity.Zero,
            tradeDate: DateTimeOffset.UtcNow);

        // Act
        await sut.ApplyTradeAsync(order, trade);

        // Assert: هیچ تعاملی با repoها نداشته باشد
        deps.Positions.Verify(x => x.AddAsync(It.IsAny<Position>()), Times.Never);
        deps.EventLogger.Verify(x => x.Log(
            It.IsAny<TradingStage>(),
            It.IsAny<TradingEventType>(),
            It.IsAny<TradingEventSeverity>(),
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<Guid?>(),
            It.IsAny<string?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static (PositionManager Sut, TestDeps Deps) CreateSut()
    {
        // InMemory DbContext برای ChangeTracker — خالی، بدون tracking
        var context = new TraderDbContext(
            new DbContextOptionsBuilder<TraderDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var positions = new Mock<IRepository<TraderDbContext, Position, Guid>>();
        var positionSpecs = new Mock<ISpecificationRepository<Position, Guid>>();
        var orderIntents = new Mock<IRepository<TraderDbContext, OrderIntent, Guid>>();
        var tradingRuns = new Mock<IRepository<TraderDbContext, TradingRun, Guid>>();

        // Default: empty spec result (per-test override for existing position)
        positionSpecs.Setup(x => x.ListBySpecAsync(
                It.IsAny<Core.Domain.Specifications.ISpecification<Position>>()))
            .ReturnsAsync(Enumerable.Empty<Position>());

        var uow = new Mock<ITraderUnitOfWork<TraderDbContext>>();
        uow.SetupGet(x => x.Context).Returns(context);
        uow.SetupGet(x => x.Positions).Returns(positions.Object);
        uow.SetupGet(x => x.PositionSpecs).Returns(positionSpecs.Object);
        uow.SetupGet(x => x.OrderIntents).Returns(orderIntents.Object);
        uow.SetupGet(x => x.TradingRuns).Returns(tradingRuns.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var eventLogger = new Mock<ITradingEventLogger>();
        var runCache = new Mock<IRunExecutionContextCache>();

        var sut = new PositionManager(
            uow.Object,
            eventLogger.Object,
            runCache.Object,
            NullLogger<PositionManager>.Instance);

        return (sut, new TestDeps(
            positions, positionSpecs, orderIntents, tradingRuns,
            eventLogger, runCache, context));
    }

    private sealed record TestDeps(
        Mock<IRepository<TraderDbContext, Position, Guid>> Positions,
        Mock<ISpecificationRepository<Position, Guid>> PositionSpecs,
        Mock<IRepository<TraderDbContext, OrderIntent, Guid>> OrderIntents,
        Mock<IRepository<TraderDbContext, TradingRun, Guid>> TradingRuns,
        Mock<ITradingEventLogger> EventLogger,
        Mock<IRunExecutionContextCache> RunCache,
        TraderDbContext Context);

    private static BrokerOrder CreateOrder(OrderSide side, string isin = "TEST")
    {
        return BrokerOrder.CreatePending(
            runId: Guid.NewGuid(),
            traderAccountId: Guid.NewGuid(),
            orderIntentId: null,
            isin: isin,
            side: side,
            price: new Price(1000m),
            quantity: new Quantity(100),
            isPaper: false);
    }

    private static OrderTrade CreateTrade(
        BrokerOrder order, OrderSide side, long qty, decimal price)
    {
        return OrderTrade.Create(
            brokerOrderId: order.Id,
            tradeNumber: 1,
            isin: order.Isin,
            side: side,
            price: new Price(price),
            quantity: new Quantity(qty),
            tradeDate: DateTimeOffset.UtcNow);
    }

    private static TradingRun CreateRunningRun(Guid accountId)
    {
        var run = TradingRun.Create(
            name: "test run",
            ownerPersonId: Guid.NewGuid(),
            strategyKey: "rsi",
            strategyConfigJson: "{\"period\":14}",
            timeFrame: TimeFrame.D1,
            capitalAllocation: CapitalAllocation.Dynamic(),
            stopLoss: null,
            takeProfit: null,
            defaultOrderMode: OrderMode.Quantity,
            onMarketClose: MarketCloseBehavior.KeepPositions,
            onStop: StopRunBehavior.KeepPositions,
            symbols: new[] { ("TEST", new SymbolAllocation(100m)) },
            accounts: new[] { (accountId , 100m) });

        run.Start();   // Created → Ready → Running
        return run;
    }
}