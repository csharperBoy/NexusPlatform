using Trader.Domain.Enums;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Backtesting;

/// <summary>یک Entry Fill از Staged Buying.</summary>
internal sealed record EntryFill(
    DateTime Time,
    decimal Price,
    long Quantity,
    decimal Commission);

/// <summary>موقعیت باز در طول Backtest.</summary>
internal sealed class OpenPosition
{
    public required string Isin { get; init; }
    public required DateTime EntryTime { get; init; }
    public required long Quantity { get; set; }
    public required decimal WeightedAvgEntryPrice { get; set; }
    public required decimal TotalBuyCommission { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public List<EntryFill> EntryFills { get; init; } = new();
}

/// <summary>سفارشی که در انتظار Fill در کندل بعدی است.</summary>
internal sealed record PendingOrder(
    string Isin,
    OrderSide Side,
    long Quantity,
    DateTime SignalTime);

/// <summary>یک رخداد در Timeline شبیه‌سازی (candle + sub-candles).
/// نام «BacktestCandleEvent» تا با Domain Entity اشتباه نشود.</summary>
internal sealed record BacktestCandleEvent(
    DateTime Time,
    string Isin,
    Candle Candle,
    IReadOnlyList<Candle>? SubCandles);

/// <summary>وضعیت جاری Backtest.</summary>
internal sealed class BacktestState
{
    public required decimal InitialCapital { get; init; }
    public decimal Cash { get; set; }
    public Dictionary<string, OpenPosition> Positions { get; } = new();
    public Dictionary<string, PendingOrder> PendingOrders { get; } = new();
    public Dictionary<string, decimal> LastPrices { get; } = new();
    public List<Domain.Entities.BacktestTrade> Trades { get; } = new();
    public List<Domain.Entities.BacktestDailyEquity> DailyEquities { get; } = new();
    public decimal TotalCommission { get; set; }
    public decimal TotalSlippage { get; set; }
}