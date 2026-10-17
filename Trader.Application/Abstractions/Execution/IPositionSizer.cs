using Trader.Application.Runtime;
using Trader.Domain.Entities;

namespace Trader.Application.Abstractions.Execution;

/// <summary>
/// از سرمایه تخصیص‌یافته به یک Symbol، تعداد سهم را محاسبه می‌کند.
/// </summary>
public interface IPositionSizer
{
    long CalculateQuantity(
        MarketSnapshot snapshot,
        Trader.Domain.Enums.SignalAction action,
        decimal allocatedCapital);
}