using Trader.Application.Abstractions.Execution;

namespace Trader.Infrastructure.Execution;

public sealed class OrderSizeCalculator : IOrderSizeCalculator
{
    public OrderSizeResult CalculateBuy(BuySizeInput input)
    {
        // ─── ۱. Target ───
        var targetCapital = input.AllocatedCapital
            * input.SignalStrength
            * input.StrengthMultiplier;

        if (targetCapital > input.AllocatedCapital)
            targetCapital = input.AllocatedCapital;   // cap

        // ─── ۲. Delta ───
        var delta = targetCapital - input.CommittedCapital;

        if (delta <= 0m)
            return new OrderSizeResult(0, targetCapital, input.CommittedCapital, delta,
                "DeltaLessThanOrEqualZero");

        // ─── ۳. MinDelta ───
        var minDelta = input.AllocatedCapital * input.MinDeltaPercent;
        if (minDelta > 0m && delta < minDelta)
            return new OrderSizeResult(0, targetCapital, input.CommittedCapital, delta,
                "DeltaBelowMinThreshold");

        // ─── ۴. Commission ───
        var effectiveDelta = delta * (1m - input.BuyCommissionRate);

        // ─── ۵. Quantity ───
        if (input.Price <= 0m)
            return new OrderSizeResult(0, targetCapital, input.CommittedCapital, delta,
                "InvalidPrice");

        var qty = (long)Math.Floor(effectiveDelta / input.Price);

        // ─── ۶. AvailableCash Guard (اختیاری) ───
        if (input.AvailableCash > 0m && qty > 0)
        {
            var cost = qty * input.Price * (1m + input.BuyCommissionRate);
            if (cost > input.AvailableCash)
            {
                var maxAffordable = (long)Math.Floor(
                    input.AvailableCash / (input.Price * (1m + input.BuyCommissionRate)));
                if (maxAffordable < qty)
                    qty = maxAffordable;
            }
        }

        // ─── ۷. Min Value ───
        if (input.MinOrderValueRial.HasValue && qty > 0)
        {
            var orderValue = qty * (long)input.Price;
            if (orderValue < input.MinOrderValueRial.Value)
                return new OrderSizeResult(0, targetCapital, input.CommittedCapital, delta,
                    "OrderValueBelowMin");
        }

        // ─── ۸. Min Quantity ───
        if (qty < input.MinOrderQuantity)
            return new OrderSizeResult(0, targetCapital, input.CommittedCapital, delta,
                "QuantityBelowMin");

        // ─── ۹. Max Quantity (Cap) ───
        if (qty > input.MaxOrderQuantity)
            qty = input.MaxOrderQuantity;

        return new OrderSizeResult(qty, targetCapital, input.CommittedCapital, delta, null);
    }

    public OrderSizeResult CalculateSell(SellSizeInput input)
    {
        if (input.PositionQuantity <= 0)
            return new OrderSizeResult(0, 0m, 0m, 0m, "NoPosition");

        if (input.PositionQuantity < input.MinOrderQuantity)
            return new OrderSizeResult(0, 0m, 0m, 0m, "PositionQtyBelowMin");

        var qty = input.PositionQuantity;
        if (qty > input.MaxOrderQuantity)
            qty = input.MaxOrderQuantity;

        return new OrderSizeResult(qty, 0m, 0m, 0m, null);
    }
}