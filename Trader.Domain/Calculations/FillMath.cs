namespace Trader.Domain.Calculations;

/// <summary>
/// محاسبات خالص Fill که بین Live/Paper و Backtest مشترک است.
///
/// این class عمداً static است (نه interface) چون:
///   1. Pure arithmetic — هیچ I/O، هیچ state، هیچ وابستگی
///   2. Static = zero DI ceremony، JIT inline-able
///   3. Unit-testable بدون Mock
///
/// Callers:
///   • Position.ApplyBuyFill / ApplySellFill (Live/Paper gross tracking)
///   • BacktestTrade.Create (Backtest gross PnL)
///   • BacktestEngine.ExecuteFill / ClosePosition (Backtest full accounting)
///
/// Semantic notes:
///   • CalculateRealizedPnL → gross (بدون کسر commission). Position این را
///     accumulate می‌کند؛ Net PnL باید در caller محاسبه شود (BacktestTrade
///     این کار را می‌کند).
///   • CalculateCommission → بدون rounding. Precision در Storage Layer
///     (decimal(18,4)) اعمال می‌شود.
/// </summary>
public static class FillMath
{
    /// <summary>
    /// قیمت میانگین وزن‌دار بعد از یک Fill خرید.
    ///
    /// فرمول: (currentQty × currentAvg + fillQty × fillPrice) / (currentQty + fillQty)
    ///
    /// اگر currentQty == 0 (خرید اول)، مقدار برگشتی دقیقاً fillPrice است.
    /// </summary>
    public static decimal CalculateWeightedAveragePrice(
        long currentQty,
        decimal currentAvgPrice,
        long fillQty,
        decimal fillPrice)
    {
        if (currentQty < 0)
            throw new ArgumentOutOfRangeException(nameof(currentQty),
                "Current quantity cannot be negative.");
        if (fillQty <= 0)
            throw new ArgumentOutOfRangeException(nameof(fillQty),
                "Fill quantity must be positive.");
        if (fillPrice < 0m)
            throw new ArgumentOutOfRangeException(nameof(fillPrice),
                "Fill price cannot be negative.");
        if (currentAvgPrice < 0m)
            throw new ArgumentOutOfRangeException(nameof(currentAvgPrice),
                "Current average price cannot be negative.");

        // خرید اول روی Position خالی
        if (currentQty == 0)
            return fillPrice;

        var newQty = currentQty + fillQty;
        return ((currentQty * currentAvgPrice) + (fillQty * fillPrice)) / newQty;
    }

    /// <summary>
    /// PnL محقق‌شده (gross — بدون کسر commission) برای یک Fill فروش.
    ///
    /// فرمول: (fillPrice - avgPrice) × fillQty
    ///
    /// Note: این gross است — commission کسر نشده. Net PnL باید در caller
    /// محاسبه شود (BacktestTrade این کار را می‌کند).
    /// </summary>
    public static decimal CalculateRealizedPnL(
        decimal avgPrice,
        decimal fillPrice,
        long fillQty)
    {
        if (fillQty <= 0)
            throw new ArgumentOutOfRangeException(nameof(fillQty),
                "Fill quantity must be positive.");
        if (fillPrice < 0m)
            throw new ArgumentOutOfRangeException(nameof(fillPrice),
                "Fill price cannot be negative.");
        if (avgPrice < 0m)
            throw new ArgumentOutOfRangeException(nameof(avgPrice),
                "Average price cannot be negative.");

        return (fillPrice - avgPrice) * fillQty;
    }
    /// <summary>
    /// حداکثر تعداد سهمی که با capital مشخص، قیمت مشخص، و commission rate مشخص
    /// می‌توان خرید — با لحاظ commission روی total cost.
    ///
    /// فرمول: floor(capital / (price × (1 + commissionRate)))
    ///
    /// این فرمول دقیق است: qty × price + qty × price × c = qty × price × (1+c) ≤ capital
    ///
    /// Edge cases:
    ///   • capital <= 0 → 0 (هیچ خرید ممکن نیست)
    ///   • price <= 0 → throw (invalid)
    ///   • commissionRate خارج [0, 0.1] → throw (invalid)
    /// </summary>
    public static long CalculateMaxBuyQuantity(
        decimal capital,
        decimal price,
        decimal commissionRate)
    {
        if (price <= 0m)
            throw new ArgumentOutOfRangeException(nameof(price),
                "Price must be positive.");
        if (commissionRate < 0m || commissionRate > 0.1m)
            throw new ArgumentOutOfRangeException(nameof(commissionRate),
                "Commission rate must be between 0 and 0.1 (0%-10%).");

        if (capital <= 0m)
            return 0L;

        return (long)Math.Floor(capital / (price * (1m + commissionRate)));
    }
    /// <summary>
    /// Commission برای یک Fill (خرید یا فروش — هر دو یک فرمول).
    ///
    /// فرمول: fillPrice × fillQty × commissionRate
    /// </summary>
    public static decimal CalculateCommission(
        decimal fillPrice,
        long fillQty,
        decimal commissionRate)
    {
        if (fillQty <= 0)
            throw new ArgumentOutOfRangeException(nameof(fillQty),
                "Fill quantity must be positive.");
        if (fillPrice < 0m)
            throw new ArgumentOutOfRangeException(nameof(fillPrice),
                "Fill price cannot be negative.");
        if (commissionRate < 0m || commissionRate > 0.1m)
            throw new ArgumentOutOfRangeException(nameof(commissionRate),
                "Commission rate must be between 0 and 0.1 (0%-10%).");

        return fillPrice * fillQty * commissionRate;
    }
}