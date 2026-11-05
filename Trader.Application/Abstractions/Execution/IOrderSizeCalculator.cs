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
    decimal AllocatedCapital,          // totalCapital × allocation%
    decimal CommittedCapital,          // هزینه‌ی Positionهای باز + Orderهای در راه
    decimal? AvailableCash,             // 0 = چک نشود (Live)؛ در Backtest = Cash واقعی
    decimal SignalStrength,            // 0..1
    decimal StrengthMultiplier,
    decimal MinDeltaPercent,
    decimal Price,
    decimal BuyCommissionRate,
    long MinOrderQuantity,
    long MaxOrderQuantity,
    long? MinOrderValueRial);

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