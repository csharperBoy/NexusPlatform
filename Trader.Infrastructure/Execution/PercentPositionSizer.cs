using Trader.Application.Abstractions.Execution;
using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Execution;

/// <summary>
/// Quantity = floor(Capital / Price)
/// با کسر کارمزد تقریبی.
/// </summary>
public sealed class PercentPositionSizer : IPositionSizer
{
    // کارمزد تقریبی — اگر Broker اجازه بدهد، از Preset می‌آید
    private const decimal DefaultCommissionRate = 0.01m; // ۱٪

    public long CalculateQuantity(
        MarketSnapshot snapshot,
        SignalAction action,
        decimal allocatedCapital)
    {
        var price = ResolvePrice(snapshot, action);
        if (price <= 0m) return 0;

        // Capital را برای کارمزد تعدیل می‌کنیم
        var effectiveCapital = allocatedCapital * (1m - DefaultCommissionRate);

        var qty = (long)Math.Floor(effectiveCapital / price);
        return qty < 0 ? 0 : qty;
    }

    private static decimal ResolvePrice(MarketSnapshot snapshot, SignalAction action)
    {
        if (action == SignalAction.Buy)
        {
            if (snapshot.BestSellPrice > 0) return snapshot.BestSellPrice;
            return snapshot.LastTradedPrice > 0 ? snapshot.LastTradedPrice : 0m;
        }

        if (snapshot.BestBuyPrice > 0) return snapshot.BestBuyPrice;
        return snapshot.LastTradedPrice > 0 ? snapshot.LastTradedPrice : 0m;
    }
}