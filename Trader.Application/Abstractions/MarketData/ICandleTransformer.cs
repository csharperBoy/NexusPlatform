using Trader.Application.Runtime;

namespace Trader.Application.Abstractions.MarketData;

public sealed record CandleTransformationResult(
    IReadOnlyList<Candle> Candles,
    decimal? LastCandleRate,
    DateTime? LastCandleRateEffectiveAt,
    int CandleCountBeforeFirstRate,
    int RatePointCount);

public interface ICandleTransformer
{
    Task<CandleTransformationResult> TransformAsync(
        string isin,
        IReadOnlyList<Candle> irrCandles,
        string targetCurrency,
        DateTime atTime,
        CancellationToken ct = default);
}