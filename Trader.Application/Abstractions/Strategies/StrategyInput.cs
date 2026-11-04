using Trader.Application.Runtime;
using Trader.Domain.Enums;

namespace Trader.Application.Abstractions.Strategies;

public sealed record StrategyInput(
    string Isin,
    MarketSnapshot? LatestSnapshot,
    IReadOnlyList<Candle> Candles,

    /// <summary>
    /// کندل‌های Sub-TimeFrame. Key = TimeFrame درخواستی از ExtraCandleRequirements.
    /// null یا خالی = Strategy فقط PrimaryTimeFrame را می‌بیند.
    /// </summary>
    IReadOnlyDictionary<TimeFrame, IReadOnlyList<Candle>>? ExtraTimeFrames,

    StrategyConfig Configuration,
    TimeFrame TimeFrame,
    DateTime EvaluatedAt);