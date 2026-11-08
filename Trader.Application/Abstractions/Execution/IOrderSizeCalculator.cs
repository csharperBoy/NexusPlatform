namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// محاسبه‌ی خالص حجم سفارش — بدون I/O، بدون DbContext.
/// در Live و Backtest از همان منطق استفاده می‌شود.
/// </summary>
public interface IOrderSizeCalculator
{
    OrderSizeResult CalculateBuy(BuySizeInput input);
    OrderSizeResult CalculateSell(SellSizeInput input);
}

public sealed record BuySizeInput(
    decimal AllocatedCapital,
    decimal CommittedCapital,
    decimal? AvailableCash,
    decimal SignalStrength,
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    decimal Price,
    decimal BuyCommissionRate,
    long MinOrderQuantity,
    long MaxOrderQuantity,
    long? MinOrderValueRial,

    /// <summary>
    /// سقف مطلق ارزش Position برای این (Run, Account, Symbol).
    /// null = بدون سقف. اگر Target از این مقدار بیشتر شد، Cap می‌شود.
    /// Delta برای رسیدن به Max از Committed محاسبه می‌شود.
    /// </summary>
    decimal? MaxPositionValue = null);

public sealed record SellSizeInput(
    long PositionQuantity,
    long MinOrderQuantity,
    long MaxOrderQuantity);

public sealed record OrderSizeResult(
    long Quantity,
    decimal TargetCapital,
    decimal CommittedCapital,
    decimal Delta,
    string? SkipReason);   // null = valid