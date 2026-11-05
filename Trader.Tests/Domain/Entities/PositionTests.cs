using FluentAssertions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Xunit;

namespace Trader.Tests.Domain.Entities;

public class PositionTests
{
    private static Position CreateOpenPosition(
        long qty = 100, decimal price = 1_000m)
    {
        return Position.Open(
            runId: Guid.NewGuid(),
            traderAccountId: Guid.NewGuid(),
            isin: "TEST",
            quantity: new Quantity(qty),
            entryPrice: new Price(price),
            stopLossPrice: null,
            takeProfitPrice: null);
    }

    [Fact]
    public void Open_WithValidData_SetsStateOpen()
    {
        var pos = CreateOpenPosition();

        pos.State.Should().Be(PositionState.Open);
        pos.Quantity.Value.Should().Be(100);
        pos.AverageEntryPrice.Value.Should().Be(1_000m);
        pos.RealizedPnL.Amount.Should().Be(0m);
        pos.ClosedAt.Should().BeNull();
    }

    [Fact]
    public void Open_WithZeroQuantity_Throws()
    {
        var act = () => Position.Open(
            Guid.NewGuid(), Guid.NewGuid(), "TEST",
            new Quantity(0), new Price(100), null, null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ApplyBuyFill_SecondFill_RecalculatesWeightedAverage()
    {
        var pos = CreateOpenPosition(qty: 100, price: 1_000m);

        // +100 @ 2,000 → avg = (100×1000 + 100×2000)/200 = 1,500
        pos.ApplyBuyFill(new Quantity(100), new Price(2_000m));

        pos.Quantity.Value.Should().Be(200);
        pos.AverageEntryPrice.Value.Should().Be(1_500m);
    }

    [Fact]
    public void ApplySellFill_PartialClose_ReducesQuantity_AddsPnL()
    {
        var pos = CreateOpenPosition(qty: 100, price: 1_000m);

        // sell 40 @ 1,500 → PnL = 40 × 500 = 20,000
        pos.ApplySellFill(new Quantity(40), new Price(1_500m));

        pos.Quantity.Value.Should().Be(60);
        pos.RealizedPnL.Amount.Should().Be(20_000m);
        pos.State.Should().Be(PositionState.PartiallyClosed);
    }

    [Fact]
    public void ApplySellFill_FullClose_SetsStateClosed()
    {
        var pos = CreateOpenPosition(qty: 100, price: 1_000m);

        pos.ApplySellFill(new Quantity(100), new Price(1_100m));

        pos.Quantity.Value.Should().Be(0);
        pos.State.Should().Be(PositionState.Closed);
        pos.ClosedAt.Should().NotBeNull();
        pos.RealizedPnL.Amount.Should().Be(10_000m);
    }

    [Fact]
    public void ApplySellFill_ExceedingQuantity_Throws()
    {
        var pos = CreateOpenPosition(qty: 100, price: 1_000m);

        var act = () => pos.ApplySellFill(new Quantity(150), new Price(1_500m));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*exceeds*");
    }

    [Fact]
    public void ApplySellFill_MultiplePartialSells_AccumulatesRealizedPnL()
    {
        var pos = CreateOpenPosition(qty: 100, price: 1_000m);

        pos.ApplySellFill(new Quantity(30), new Price(1_200m));   // +6,000
        pos.ApplySellFill(new Quantity(30), new Price(1_400m));   // +12,000

        pos.RealizedPnL.Amount.Should().Be(18_000m);
        pos.Quantity.Value.Should().Be(40);
    }

    [Fact]
    public void ShouldTriggerStopLoss_WhenPriceAtOrBelow_ReturnsTrue()
    {
        var pos = Position.Open(
            Guid.NewGuid(), Guid.NewGuid(), "TEST",
            new Quantity(100), new Price(1_000m),
            stopLossPrice: new Price(900m),
            takeProfitPrice: null);

        pos.ShouldTriggerStopLoss(new Price(900m)).Should().BeTrue();
        pos.ShouldTriggerStopLoss(new Price(899m)).Should().BeTrue();
        pos.ShouldTriggerStopLoss(new Price(901m)).Should().BeFalse();
    }

    [Fact]
    public void ShouldTriggerTakeProfit_WhenPriceAtOrAbove_ReturnsTrue()
    {
        var pos = Position.Open(
            Guid.NewGuid(), Guid.NewGuid(), "TEST",
            new Quantity(100), new Price(1_000m),
            stopLossPrice: null,
            takeProfitPrice: new Price(1_200m));

        pos.ShouldTriggerTakeProfit(new Price(1_200m)).Should().BeTrue();
        pos.ShouldTriggerTakeProfit(new Price(1_201m)).Should().BeTrue();
        pos.ShouldTriggerTakeProfit(new Price(1_199m)).Should().BeFalse();
    }
}