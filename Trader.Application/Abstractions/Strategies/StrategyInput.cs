using Trader.Application.Runtime;
using Trader.Domain.Enums;
namespace Trader.Application.Abstractions.Strategies;

/// <summary>
/// تمام ورودی‌های موردنیاز یک Strategy در یک لحظه.
/// Strategy هیچ I/O ندارد — همه چیز از پیش آماده است.
/// </summary>
public sealed record StrategyInput(
    string Isin,
    MarketSnapshot LatestSnapshot,
    IReadOnlyList<Candle> Candles,
    StrategyConfig Configuration,
    TimeFrame TimeFrame,
    DateTime EvaluatedAt);
