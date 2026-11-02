using Trader.Application.Abstractions.Backtesting;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Backtesting;

public sealed class OrderFillSimulator : IOrderFillSimulator
{
    public decimal FillPrice(
        OrderSide side,
        decimal rawPrice,
        decimal slippagePercent)
    {
        if (rawPrice <= 0m) return rawPrice;

        return side == OrderSide.Buy
            ? rawPrice * (1m + slippagePercent)
            : rawPrice * (1m - slippagePercent);
    }
}