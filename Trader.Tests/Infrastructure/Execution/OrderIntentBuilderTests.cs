using Core.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Execution;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Instruments;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.Execution;
using Xunit;

namespace Trader.Tests.Infrastructure.Execution;

public class OrderIntentBuilderTests
{
    [Fact]
    public async Task BuildAsync_MaxPositionSizeEnabled_PassesComputedValueToCalculator()
    {
        var (sut, sizeCalc) = CreateSut(
            maxPosEnabled: true, maxPercent: 30m, capital: 100_000_000m);

        await sut.BuildAsync(BuildSignal(), BuildContext(), Guid.NewGuid());

        sizeCalc.Verify(x => x.CalculateBuy(It.Is<BuySizeInput>(
            i => i.MaxPositionValue == 30_000_000m)), Times.Once);
    }

    [Fact]
    public async Task BuildAsync_MaxPositionSizeDisabled_PassesNull()
    {
        var (sut, sizeCalc) = CreateSut(
            maxPosEnabled: false, maxPercent: 30m, capital: 100_000_000m);

        await sut.BuildAsync(BuildSignal(), BuildContext(), Guid.NewGuid());

        sizeCalc.Verify(x => x.CalculateBuy(It.Is<BuySizeInput>(
            i => i.MaxPositionValue == null)), Times.Once);
    }

    [Fact]
    public async Task BuildAsync_CalculatorReturnsCapReached_EmitsEventAndReturnsNull()
    {
        var (sut, sizeCalc, eventLogger) = CreateSutFull(
            maxPosEnabled: true, maxPercent: 30m, capital: 100_000_000m);

        sizeCalc.Setup(x => x.CalculateBuy(It.IsAny<BuySizeInput>()))
            .Returns(new OrderSizeResult(0, 30_000_000m, 30_000_000m, 0m,
                "PositionValueCapReached"));

        var result = await sut.BuildAsync(
            BuildSignal(), BuildContext(), Guid.NewGuid());

        result.Should().BeNull();

        eventLogger.Verify(x => x.Log(
            TradingStage.Risk,
            TradingEventType.MaxPositionSizeReached,
            It.IsAny<TradingEventSeverity>(),
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>()),
            Times.Once);
    }

    // ─── Helpers ───

    private static (OrderIntentBuilder Sut, Mock<IOrderSizeCalculator> SizeCalc)
    CreateSut(bool maxPosEnabled, decimal maxPercent, decimal capital)
    {
        var (sut, sizeCalc, _) = CreateSutFull(maxPosEnabled, maxPercent, capital);
        return (sut, sizeCalc);
    }

    private static (OrderIntentBuilder Sut,
                    Mock<IOrderSizeCalculator> SizeCalc,
                    Mock<ITradingEventLogger> EventLogger)
        CreateSutFull(bool maxPosEnabled, decimal maxPercent, decimal capital)
    {
        var snapshotStore = new Mock<IMarketSnapshotStore>();
        snapshotStore.Setup(x => x.GetLatest("TEST"))
            .Returns(MakeSnapshot("TEST"));

        var capitalAllocator = new Mock<ICapitalAllocator>();
        capitalAllocator.Setup(x => x.GetAllocatableCapitalAsync(
                It.IsAny<RunRuntimeContext>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(capital);

        var committedCalc = new Mock<ICommittedCapitalCalculator>();
        committedCalc.Setup(x => x.CalculateForSymbolAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var sizeCalc = new Mock<IOrderSizeCalculator>();
        sizeCalc.Setup(x => x.CalculateBuy(It.IsAny<BuySizeInput>()))
            .Returns(new OrderSizeResult(100, 1_000m, 0m, 1_000m, null));

        var uow = new Mock<ITraderUnitOfWork<TraderDbContext>>();
        var instrumentSpecs = new Mock<ISpecificationRepository<Instrument, Guid>>();
        instrumentSpecs.Setup(x => x.GetBySpecAsync(
                It.IsAny<InstrumentByIsinSpec>()))
            .ReturnsAsync(MakeInstrument());
        uow.SetupGet(x => x.InstrumentSpecs).Returns(instrumentSpecs.Object);

        // ─── OrderIntents repository (OrderIntentBuilder.AddAsync) ───
        var orderIntents = new Mock<IRepository<TraderDbContext, OrderIntent, Guid>>();
        orderIntents.Setup(x => x.AddAsync(It.IsAny<OrderIntent>()))
            .Returns(Task.CompletedTask);
        uow.SetupGet(x => x.OrderIntents).Returns(orderIntents.Object);

        var eventLogger = new Mock<ITradingEventLogger>();

        var riskOptions = new Mock<IOptionsMonitor<TradingRiskOptions>>();
        riskOptions.Setup(x => x.CurrentValue).Returns(new TradingRiskOptions
        {
            Enabled = true,
            MaxPositionSize = new MaxPositionSizeOptions
            {
                Enabled = maxPosEnabled,
                MaxPercentPerSymbol = maxPercent,
                ApplyToPaper = true,
            }
        });

        var sut = new OrderIntentBuilder(
            snapshotStore.Object,
            capitalAllocator.Object,
            committedCalc.Object,
            sizeCalc.Object,
            uow.Object,
            eventLogger.Object,
            riskOptions.Object,
            NullLogger<OrderIntentBuilder>.Instance);

        return (sut, sizeCalc, eventLogger);
    }

    private static Signal BuildSignal()
    {
        return Signal.Create(
            runId: Guid.NewGuid(),
            isin: "TEST",
            strategyKey: "rsi",
            action: SignalAction.Buy,
            strength: new SignalStrength(1.0m));
    }

    private static RunRuntimeContext BuildContext()
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
            AccountAllocations: new[] { new RunAccountAllocation(Guid.NewGuid(), AccountCapitalAllocation.Dynamic(100m)) });
    }

    private static MarketSnapshot MakeSnapshot(string isin) => new(
        Id: Guid.NewGuid(),
        Isin: isin,
        SymbolName: isin,
        StateCode: "OPEN",
        LastTradedPrice: 1_000L,
        ClosingPrice: 1_000L,
        PreviousDayClosingPrice: 990L,
        PriceVar: 0.01,
        FirstTradedPrice: 1_000L,
        HighPrice: 1_010L,
        LowPrice: 990L,
        HighAllowedPrice: 1_050L,
        LowAllowedPrice: 950L,
        PriceTickSize: 1L,
        BestBuyPrice: 999L,
        BestBuyQuantity: 100L,
        BestSellPrice: 1_001L,
        BestSellQuantity: 100L,
        BuyRatio: 1.5m,
        SellRatio: 1.0m,
        MinValidBuyVolume: 1L,
        MaxValidBuyVolume: 100_000L,
        MinValidSellVolume: 1L,
        MaxValidSellVolume: 100_000L,
        TotalTradeValue: 10_000_000L,
        TotalNumberOfTrades: 500L,
        TotalNumberOfSharesTraded: 10_000L,
        CapturedAt: DateTime.UtcNow,
        Source: "test");

    private static Instrument MakeInstrument()
    {
        return Instrument.Create(
            isin: "TEST",
            symbolName: "TEST",
            displayName: "Test",
            type: InstrumentType.Stock,
            market: MarketType.IranStock,
            buyCommissionRate: 0m,
            sellCommissionRate: 0m);
    }
}