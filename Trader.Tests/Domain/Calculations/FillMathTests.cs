using FluentAssertions;
using Trader.Domain.Calculations;
using Xunit;

namespace Trader.Tests.Domain.Calculations;

public class FillMathTests
{
    // ═══════════════════════════════════════════════════════════════
    // CalculateWeightedAveragePrice
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void WeightedAveragePrice_FirstBuy_ReturnsFillPrice()
    {
        var avg = FillMath.CalculateWeightedAveragePrice(
            currentQty: 0, currentAvgPrice: 0m,
            fillQty: 100, fillPrice: 1_000m);

        avg.Should().Be(1_000m);
    }

    [Fact]
    public void WeightedAveragePrice_SameBuyPrice_ReturnsSamePrice()
    {
        var avg = FillMath.CalculateWeightedAveragePrice(
            currentQty: 100, currentAvgPrice: 1_000m,
            fillQty: 50, fillPrice: 1_000m);

        avg.Should().Be(1_000m);
    }

    [Fact]
    public void WeightedAveragePrice_HigherSecondBuy_IncreasesAverage()
    {
        // (100×1000 + 100×2000) / 200 = 1,500
        var avg = FillMath.CalculateWeightedAveragePrice(
            currentQty: 100, currentAvgPrice: 1_000m,
            fillQty: 100, fillPrice: 2_000m);

        avg.Should().Be(1_500m);
    }

    [Fact]
    public void WeightedAveragePrice_LowerSecondBuy_DecreasesAverage()
    {
        // (200×1000 + 100×500) / 300 = 833.33...
        var avg = FillMath.CalculateWeightedAveragePrice(
            currentQty: 200, currentAvgPrice: 1_000m,
            fillQty: 100, fillPrice: 500m);

        avg.Should().BeApproximately(833.3333m, 0.0001m);
    }

    [Fact]
    public void WeightedAveragePrice_AsymmetricQuantities_WeightsCorrectly()
    {
        // (10×100 + 90×200) / 100 = (1000 + 18000)/100 = 190
        var avg = FillMath.CalculateWeightedAveragePrice(
            currentQty: 10, currentAvgPrice: 100m,
            fillQty: 90, fillPrice: 200m);

        avg.Should().Be(190m);
    }

    [Fact]
    public void WeightedAveragePrice_ZeroFillQty_Throws()
    {
        var act = () => FillMath.CalculateWeightedAveragePrice(
            currentQty: 100, currentAvgPrice: 1000m,
            fillQty: 0, fillPrice: 2000m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fillQty");
    }

    [Fact]
    public void WeightedAveragePrice_NegativeFillPrice_Throws()
    {
        var act = () => FillMath.CalculateWeightedAveragePrice(
            currentQty: 100, currentAvgPrice: 1000m,
            fillQty: 50, fillPrice: -1m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fillPrice");
    }

    // ═══════════════════════════════════════════════════════════════
    // CalculateRealizedPnL
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void RealizedPnL_PriceHigherThanAvg_ReturnsPositive()
    {
        var pnl = FillMath.CalculateRealizedPnL(
            avgPrice: 1_000m, fillPrice: 1_500m, fillQty: 100);

        pnl.Should().Be(50_000m);   // (1500 - 1000) × 100
    }

    [Fact]
    public void RealizedPnL_PriceLowerThanAvg_ReturnsNegative()
    {
        var pnl = FillMath.CalculateRealizedPnL(
            avgPrice: 1_000m, fillPrice: 900m, fillQty: 100);

        pnl.Should().Be(-10_000m);   // (900 - 1000) × 100
    }

    [Fact]
    public void RealizedPnL_SamePrice_ReturnsZero()
    {
        var pnl = FillMath.CalculateRealizedPnL(
            avgPrice: 1_000m, fillPrice: 1_000m, fillQty: 100);

        pnl.Should().Be(0m);
    }

    [Fact]
    public void RealizedPnL_ZeroFillQty_Throws()
    {
        var act = () => FillMath.CalculateRealizedPnL(
            avgPrice: 1_000m, fillPrice: 1_500m, fillQty: 0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fillQty");
    }

    // ═══════════════════════════════════════════════════════════════
    // CalculateCommission
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Commission_StandardRate_ComputesCorrectly()
    {
        // 100 × 1000 × 0.0037 = 370
        var commission = FillMath.CalculateCommission(
            fillPrice: 1_000m, fillQty: 100, commissionRate: 0.0037m);

        commission.Should().Be(370m);
    }

    [Fact]
    public void Commission_ZeroRate_ReturnsZero()
    {
        var commission = FillMath.CalculateCommission(
            fillPrice: 1_000m, fillQty: 100, commissionRate: 0m);

        commission.Should().Be(0m);
    }

    [Fact]
    public void Commission_AboveMaxRate_Throws()
    {
        var act = () => FillMath.CalculateCommission(
            fillPrice: 1_000m, fillQty: 100, commissionRate: 0.15m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("commissionRate");
    }

    [Fact]
    public void Commission_NegativeRate_Throws()
    {
        var act = () => FillMath.CalculateCommission(
            fillPrice: 1_000m, fillQty: 100, commissionRate: -0.01m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("commissionRate");
    }

    // ═══════════════════════════════════════════════════════════════
    // Equivalence Tests — تایید این‌که استخراج، رفتار را تغییر نداده
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void RealizedPnL_MatchesLegacyInlineFormula()
    {
        // فرمول قبلی (Position.ApplySellFill و BacktestTrade.Create):
        //   var realized = (price.Value - AverageEntryPrice.Value) * qty.Value;
        //   var grossPnl = (exitPrice - weightedAvgEntryPrice) * exitQuantity;
        //
        // بعد از استخراج، هر دو باید همان مقدار را بدهند.
        var avgPrice = 1_234.5m;
        var fillPrice = 1_567.89m;
        var qty = 42L;

        var legacy = (fillPrice - avgPrice) * qty;
        var extracted = FillMath.CalculateRealizedPnL(avgPrice, fillPrice, qty);

        extracted.Should().Be(legacy);
    }

    [Fact]
    public void WeightedAveragePrice_MatchesLegacyInlineFormula()
    {
        // فرمول قبلی (Position.ApplyBuyFill و BacktestEngine.ExecuteFill):
        //   var newTotalQty = Quantity.Value + qty.Value;
        //   var newAvg = ((AverageEntryPrice.Value * Quantity.Value)
        //                 + (price.Value * qty.Value)) / newTotalQty;
        var currentQty = 75L;
        var currentAvg = 987.65m;
        var fillQty = 125L;
        var fillPrice = 1_234.56m;

        var legacy = ((currentAvg * currentQty) + (fillPrice * fillQty))
                     / (currentQty + fillQty);
        var extracted = FillMath.CalculateWeightedAveragePrice(
            currentQty, currentAvg, fillQty, fillPrice);

        extracted.Should().Be(legacy);
    }

    // ═══════════════════════════════════════════════════════════════
    // CalculateMaxBuyQuantity
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void MaxBuyQuantity_NoCommission_ReturnsFloorOfCapitalOverPrice()
    {
        var qty = FillMath.CalculateMaxBuyQuantity(
            capital: 1_000m, price: 100m, commissionRate: 0m);

        qty.Should().Be(10);
    }

    [Fact]
    public void MaxBuyQuantity_WithCommission_ReducesQuantity()
    {
        // 1000 / (100 * 1.1) = 1000 / 110 = 9.09... → 9
        var qty = FillMath.CalculateMaxBuyQuantity(
            capital: 1_000m, price: 100m, commissionRate: 0.1m);

        qty.Should().Be(9);
    }

    [Fact]
    public void MaxBuyQuantity_CapitalTooSmallForOneShare_ReturnsZero()
    {
        // 100 / (100 * 1.0037) = 0.9963... → 0
        var qty = FillMath.CalculateMaxBuyQuantity(
            capital: 100m, price: 100m, commissionRate: 0.0037m);

        qty.Should().Be(0);
    }

    [Fact]
    public void MaxBuyQuantity_JustEnoughForOneShare_ReturnsOne()
    {
        // 101 / (100 * 1.0037) = 101 / 100.37 = 1.0062... → 1
        var qty = FillMath.CalculateMaxBuyQuantity(
            capital: 101m, price: 100m, commissionRate: 0.0037m);

        qty.Should().Be(1);
    }

    [Fact]
    public void MaxBuyQuantity_ZeroCapital_ReturnsZero()
    {
        var qty = FillMath.CalculateMaxBuyQuantity(
            capital: 0m, price: 100m, commissionRate: 0.0037m);

        qty.Should().Be(0);
    }

    [Fact]
    public void MaxBuyQuantity_NegativeCapital_ReturnsZero()
    {
        var qty = FillMath.CalculateMaxBuyQuantity(
            capital: -100m, price: 100m, commissionRate: 0.0037m);

        qty.Should().Be(0);
    }

    [Fact]
    public void MaxBuyQuantity_ZeroPrice_Throws()
    {
        var act = () => FillMath.CalculateMaxBuyQuantity(
            capital: 1_000m, price: 0m, commissionRate: 0m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("price");
    }

    [Fact]
    public void MaxBuyQuantity_InvalidCommissionRate_Throws()
    {
        var act = () => FillMath.CalculateMaxBuyQuantity(
            capital: 1_000m, price: 100m, commissionRate: 0.15m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("commissionRate");
    }

    [Fact]
    public void MaxBuyQuantity_ExactFormula_MatchesFillMathCommission()
    {
        // Verify: qty × price + commission(qty) <= capital
        // و qty+1 از capital بیشتر می‌شود
        var capital = 100_000m;
        var price = 123.45m;
        var rate = 0.0037m;

        var qty = FillMath.CalculateMaxBuyQuantity(capital, price, rate);
        var commission = FillMath.CalculateCommission(price, qty, rate);
        var totalCost = qty * price + commission;

        totalCost.Should().BeLessOrEqualTo(capital);

        // qty+1 باید از capital بیشتر شود
        var nextCommission = FillMath.CalculateCommission(price, qty + 1, rate);
        var nextTotalCost = (qty + 1) * price + nextCommission;
        nextTotalCost.Should().BeGreaterThan(capital);
    }
}