using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Backtesting;

/// <summary>
/// شبیه‌سازی Fill قیمت با Slippage.
/// همه‌ی Orderها در Open کندل بعدی Fill می‌شوند (Next-Open).
/// Slippage: Buy × (1+slip) ، Sell × (1-slip).
/// </summary>
public interface IOrderFillSimulator
{
    decimal FillPrice(
        OrderSide side,
        decimal rawPrice,
        decimal slippagePercent);
}