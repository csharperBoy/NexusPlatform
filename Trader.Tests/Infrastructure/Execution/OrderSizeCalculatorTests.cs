using FluentAssertions;
using Trader.Application.Abstractions.Execution;
using Trader.Infrastructure.Execution;
using Xunit;

namespace Trader.Tests.Infrastructure.Execution;

public class OrderSizeCalculatorTests
{
    private readonly OrderSizeCalculator _sut = new();

    // ────────────────────────────────────────────────────────────
    // CalculateBuy
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateBuy_WithFullStrength_ReturnsQtyBasedOnCapital()
    {
        // 10,000,000 ریال، قیمت 1,000، commission 0، strength 1.0
        var input = BuyInput(
            allocated: 10_000_000m,
            committed: 0m,
            strength: 1.0m,
            price: 1_000m,
            commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(10_000);
        result.SkipReason.Should().BeNull();
    }

    [Fact]
    public void CalculateBuy_WithHalfStrength_ReturnsHalfQty()
    {
        var input = BuyInput(
            allocated: 10_000_000m,
            committed: 0m,
            strength: 0.5m,
            price: 1_000m,
            commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(5_000);
    }

    [Fact]
    public void CalculateBuy_WithZeroStrength_ReturnsZero()
    {
        var input = BuyInput(
            allocated: 10_000_000m, committed: 0m,
            strength: 0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("DeltaLessThanOrEqualZero");
    }

    [Fact]
    public void CalculateBuy_WhenCommittedExceedsTarget_ReturnsZero()
    {
        // Target = 10M × 0.5 × 1.0 = 5M. Committed = 6M > 5M.
        var input = BuyInput(
            allocated: 10_000_000m, committed: 6_000_000m,
            strength: 0.5m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("DeltaLessThanOrEqualZero");
    }

    [Fact]
    public void CalculateBuy_WhenDeltaBelowMinDeltaPercent_ReturnsZero()
    {
        // Target = 10M × 1.0 = 10M. Committed = 9.5M → Delta = 500K
        // MinDelta = 10M × 5% = 500K → Delta == MinDelta، رد نمی‌شود
        // ولی اگر Committed = 9.6M → Delta = 400K < 500K → رد
        var input = BuyInput(
            allocated: 10_000_000m, committed: 9_600_000m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            minDeltaPercent: 0.05m);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("DeltaBelowMinThreshold");
    }

    [Fact]
    public void CalculateBuy_Commission_ReducesEffectiveQuantity()
    {
        // 10M، قیمت 1,000، commission 1% → effectiveDelta = 10M × 0.99 = 9.9M
        // qty = floor(9.9M / 1,000) = 9,900
        var input = BuyInput(
            allocated: 10_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0.01m,
            minQty: 1, maxQty: 1_000_000, minValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(9_900);
    }

    [Fact]
    public void CalculateBuy_BelowMinQuantity_ReturnsZero()
    {
        var input = BuyInput(
            allocated: 10_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 100, maxQty: 1_000_000, minValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("QuantityBelowMin");
    }

    [Fact]
    public void CalculateBuy_AboveMaxQuantity_IsCapped()
    {
        var input = BuyInput(
            allocated: 100_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 50_000, minValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(50_000);
    }

    [Fact]
    public void CalculateBuy_BelowMinOrderValue_ReturnsZero()
    {
        // 10M، قیمت 1,000 → 10,000 سهم → value = 10,000 × 1,000 = 10M
        // MinOrderValueRial = 20M → رد
        var input = BuyInput(
            allocated: 10_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: 20_000_000m);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("OrderValueBelowMin");
    }

    [Fact]
    public void CalculateBuy_WithAvailableCash_ClampsQuantity()
    {
        // Target از capital 10M، ولی cash فقط 5M
        var input = BuyInput(
            allocated: 10_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            availableCash: 5_000_000m);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(5_000);
    }

    [Fact]
    public void CalculateBuy_WithNullAvailableCash_SkipsCashCheck()
    {
        // Live mode: AvailableCash = null → محدودیت cash اعمال نمی‌شود
        var input = BuyInput(
            allocated: 10_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            availableCash: null);   // ← nullable

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(10_000);
    }

    // ────────────────────────────────────────────────────────────
    // CalculateSell
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateSell_WithZeroPosition_ReturnsZero()
    {
        var input = new SellSizeInput(
            PositionQuantity: 0, MinOrderQuantity: 1, MaxOrderQuantity: 1_000_000);

        var result = _sut.CalculateSell(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("NoPosition");
    }

    [Fact]
    public void CalculateSell_WithPositionBelowMin_ReturnsZero()
    {
        var input = new SellSizeInput(
            PositionQuantity: 50, MinOrderQuantity: 100, MaxOrderQuantity: 1_000_000);

        var result = _sut.CalculateSell(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("PositionQtyBelowMin");
    }

    [Fact]
    public void CalculateSell_AboveMax_IsCapped()
    {
        var input = new SellSizeInput(
            PositionQuantity: 100_000, MinOrderQuantity: 1, MaxOrderQuantity: 10_000);

        var result = _sut.CalculateSell(input);

        result.Quantity.Should().Be(10_000);
    }
    [Fact]
    public void CalculateBuy_WithNonZeroCommission_UsesExactFormula()
    {
        // این تست عمداً عددی را انتخاب می‌کند که در آن
        // (1 - c) و 1/(1 + c) به integer مختلف floor می‌شوند.
        //
        // Legacy (1 - c):  73,000 × 0.9963 / 1 = 72,729.9  → 72,729
        // Exact 1/(1+c):   73,000 / 1.0037      ≈ 72,730.99 → 72,730
        //
        // تست قبلاً (با approximation) 72,729 برمی‌گرداند و FAIL می‌شد.
        // حالا (با exact) 72,730 برمی‌گرداند و PASS.
        var input = BuyInput(
            allocated: 73_000m,
            committed: 0m,
            strength: 1.0m,
            price: 1m,
            commission: 0.0037m,
            minQty: 1, maxQty: 200_000, minValue: null,
            availableCash: null);   // Live mode

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(72_730,
            because: "the exact formula 1/(1+c) is slightly tighter than (1-c)");
    }
    // ═══════════════════════════════════════════════════════════════
    // MaxPositionValue (PR 16)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void CalculateBuy_NoMaxPositionValue_NoCap()
    {
        // Target = 100M × 1.0 × 1.0 = 100M، بدون سقف → qty = 100k
        var input = BuyInput(
            allocated: 100_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            maxPositionValue: null);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(100_000);
        result.SkipReason.Should().BeNull();
    }

    [Fact]
    public void CalculateBuy_MaxPositionValueCaps_ReducesTarget()
    {
        // Target = 100M، Max = 30M → qty = 30k
        var input = BuyInput(
            allocated: 100_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            maxPositionValue: 30_000_000m);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(30_000);
        result.TargetCapital.Should().Be(30_000_000m);
        result.SkipReason.Should().BeNull();
    }

    [Fact]
    public void CalculateBuy_MaxPositionValueAlreadyAtCap_SkipsWithReason()
    {
        // Target = 100M، Max = 30M، Committed = 30M → Delta = 0، Skip
        var input = BuyInput(
            allocated: 100_000_000m, committed: 30_000_000m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            maxPositionValue: 30_000_000m);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(0);
        result.SkipReason.Should().Be("PositionValueCapReached");
    }

    [Fact]
    public void CalculateBuy_MaxPositionValuePartiallyUsed_BuysUpToCap()
    {
        // Target = 100M، Max = 30M، Committed = 20M → Delta = 10M → qty = 10k
        var input = BuyInput(
            allocated: 100_000_000m, committed: 20_000_000m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            maxPositionValue: 30_000_000m);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(10_000);
        result.SkipReason.Should().BeNull();
    }

    [Fact]
    public void CalculateBuy_MaxPositionValueHigherThanTarget_NoEffect()
    {
        // Target = 20M، Max = 30M → target (20M) lower than max (30M) → no cap
        var input = BuyInput(
            allocated: 20_000_000m, committed: 0m,
            strength: 1.0m, price: 1_000m, commission: 0m,
            minQty: 1, maxQty: 1_000_000, minValue: null,
            maxPositionValue: 30_000_000m);

        var result = _sut.CalculateBuy(input);

        result.Quantity.Should().Be(20_000);
        result.SkipReason.Should().BeNull();
    }
    // ────────────────────────────────────────────────────────────
    // Helper
    // ────────────────────────────────────────────────────────────

    private static BuySizeInput BuyInput(
    decimal allocated,
    decimal committed,
    decimal strength,
    decimal price,
    decimal commission,
    long minQty,
    long maxQty,
    decimal? minValue,
    decimal minDeltaPercent = 0m,
    decimal? availableCash = null,
    decimal? maxPositionValue = null)   // ← NEW
    {
        return new BuySizeInput(
            AllocatedCapital: allocated,
            CommittedCapital: committed,
            AvailableCash: availableCash,
            SignalStrength: strength,
            StrengthMultiplier: 1.0m,
            MinDeltaPercent: minDeltaPercent,
            Price: price,
            BuyCommissionRate: commission,
            MinOrderQuantity: minQty,
            MaxOrderQuantity: maxQty,
            MinOrderValueRial: minValue is null ? null : (long)minValue.Value,
            MaxPositionValue: maxPositionValue);
    }
}