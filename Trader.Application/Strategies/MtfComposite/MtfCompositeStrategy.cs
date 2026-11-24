using Trader.Application.Abstractions.Indicators;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Indicators.Atr;
using Trader.Application.Indicators.Ema;
using Trader.Application.Indicators.Macd;
using Trader.Application.Indicators.Rsi;
using Trader.Application.Runtime;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Application.Strategies.MtfComposite;

/// <summary>
/// Multi-TimeFrame Composite Strategy.
/// سه لایه‌ی مستقل: Trend (EMA)، Momentum (RSI+MACD)، Micro (Volume+OrderBook).
/// خروجی: Strength کامپوزیت (۰-۱) برای Buy و Sell جداگانه.
/// SL/TP پیشنهادی بر اساس ATR، ولی Run override می‌کنه.
/// </summary>
public sealed class MtfCompositeStrategy : IStrategy
{
    private readonly IIndicator<EmaConfig, decimal?> _ema;
    private readonly IIndicator<MacdConfig, MacdResult?> _macd;
    private readonly IIndicator<AtrConfig, decimal?> _atr;
    private readonly IIndicator<RsiConfig, decimal?> _rsi;
    private readonly MtfCompositeStrategyConfig _config;

    public MtfCompositeStrategy(
        IIndicator<EmaConfig, decimal?> ema,
        IIndicator<MacdConfig, MacdResult?> macd,
        IIndicator<AtrConfig, decimal?> atr,
        IIndicator<RsiConfig, decimal?> rsi,
        MtfCompositeStrategyConfig config)
    {
        _ema = ema;
        _macd = macd;
        _atr = atr;
        _rsi = rsi;
        _config = config;

        ValidateWeights(config);
    }

    public string Key => MtfCompositeStrategyRegistration.KeyName;
    public string DisplayName => MtfCompositeStrategyRegistration.DisplayNameValue;

    public int RequiredCandleCount
    {
        get
        {
            var minTrend = Math.Max(_config.EmaSlowPeriod + 50, _config.MacdSlow + _config.MacdSignal + 50);
            var minMicro = Math.Max(
                _config.AtrPeriod + 50,
                Math.Max(_config.IntraDayBaselineCandles,
                         _config.OpeningBaselineDays * 5) + 50);
            return Math.Max(minTrend, minMicro);
        }
    }

    public bool RequiresClosedCandlesOnly => true;

    public IReadOnlyList<CandleRequirement> ExtraCandleRequirements => new[]
    {
        // Trend TF (فقط اگه با primary فرق کنه — فریمورک خودش skip می‌کنه)
        new CandleRequirement(
            _config.TrendTimeFrame,
            _config.EmaSlowPeriod + 50,
            ClosedOnly: true),

        // Sub-candle (برای Intra-Candle Analysis)
        new CandleRequirement(
            _config.SubCandleTimeFrame,
            _config.SubCandleHistoryDepth,
            ClosedOnly: false),
    };

    public StrategyEvaluationResult Evaluate(StrategyInput input)
    {
        if (input.Candles.Count < RequiredCandleCount)
            return StrategyEvaluationResult.Empty();

        var tfDuration = input.TimeFrame.ToDuration();
        var closes = new List<decimal>(input.Candles.Count);
        foreach (var c in input.Candles) closes.Add(c.Close);

        // ─── 1. Trend Layer ───
        IReadOnlyList<Candle> trendCandles = _config.TrendTimeFrame == input.TimeFrame
            ? input.Candles
            : input.ExtraTimeFrames?.GetValueOrDefault(_config.TrendTimeFrame)
              ?? Array.Empty<Candle>();

        if (trendCandles.Count < _config.EmaSlowPeriod + 2)
            return StrategyEvaluationResult.Empty();

        var trendCloses = new List<decimal>(trendCandles.Count);
        foreach (var c in trendCandles) trendCloses.Add(c.Close);

        var trendEmaSlowSeries = EmaCalculator.CalculateSeries(
            trendCloses, _config.EmaSlowPeriod);

        if (trendEmaSlowSeries[^1] is null || trendEmaSlowSeries[^2] is null)
            return StrategyEvaluationResult.Empty();

        var emaSlowNow = trendEmaSlowSeries[^1]!.Value;
        var emaSlowPrev = trendEmaSlowSeries[^2]!.Value;
        var trendPrice = trendCloses[^1];

        var trendRising = emaSlowNow > emaSlowPrev;
        var trendFalling = emaSlowNow < emaSlowPrev;
        var priceAbove = trendPrice > emaSlowNow;
        var priceBelow = trendPrice < emaSlowNow;

        decimal trendBullish =
            trendRising && priceAbove ? 1.0m :
            trendRising || priceAbove ? 0.5m : 0.0m;

        decimal trendBearish =
            trendFalling && priceBelow ? 1.0m :
            trendFalling || priceBelow ? 0.5m : 0.0m;

        // ─── 2. Momentum Layer ───
        var rsiConfig = new RsiConfig { Period = _config.RsiPeriod };
        var rsiInput = new IndicatorInput(input.Candles, input.LatestSnapshot!, input.TimeFrame);
        var rsi = _rsi.Compute(rsiInput, rsiConfig);

        var macdConfig = new MacdConfig
        {
            FastPeriod = _config.MacdFast,
            SlowPeriod = _config.MacdSlow,
            SignalPeriod = _config.MacdSignal,
        };
        var (_, _, histSeries) = MacdCalculator.CalculateSeries(closes, macdConfig);

        if (histSeries[^1] is null) return StrategyEvaluationResult.Empty();

        var currentHist = histSeries[^1]!.Value;
        var prevHist = histSeries.Count >= 2 && histSeries[^2] is not null
            ? histSeries[^2]!.Value
            : currentHist;

        decimal rsiBull = 0m, rsiBear = 0m;
        if (rsi.HasValue)
        {
            if (rsi.Value >= _config.RsiBullishThreshold)
                rsiBull = Math.Min(1m, (rsi.Value - _config.RsiBullishThreshold) / 20m);
            if (rsi.Value <= _config.RsiBearishThreshold)
                rsiBear = Math.Min(1m, (_config.RsiBearishThreshold - rsi.Value) / 20m);
        }

        decimal macdBull = 0m, macdBear = 0m;
        if (currentHist > 0m)
            macdBull = currentHist > prevHist ? 1.0m : 0.5m;
        if (currentHist < 0m)
            macdBear = currentHist < prevHist ? 1.0m : 0.5m;

        var momBullish = (rsiBull + macdBull) / 2m;
        var momBearish = (rsiBear + macdBear) / 2m;

        // ─── 3. Microstructure Layer ───
        var baselineResult = ComputeVolumeBaselines(input.Candles, tfDuration);
        if (baselineResult is null) return StrategyEvaluationResult.Empty();

        var (intraDayBaseline, openingBaseline, currentIsOpening) = baselineResult.Value;
        var selectedBaseline = currentIsOpening ? openingBaseline : intraDayBaseline;
        if (selectedBaseline <= 0m) return StrategyEvaluationResult.Empty();

        var currentVolume = (decimal)input.Candles[^1].Volume;
        var volumeRatio = currentVolume / selectedBaseline;
        var volumeScore = Math.Min(1m, volumeRatio / _config.VolumeMultiplier);

        // Order Book (nullable)
        decimal? orderBookBull = null;
        decimal? orderBookBear = null;
        if (input.LatestSnapshot is { } snap)
        {
            var buyRatioScore = Math.Min(1m, snap.BuyRatio / _config.BuyRatioThreshold);
            var sellRatioScore = Math.Min(1m, snap.SellRatio / _config.SellRatioThreshold);

            var bidDepth = (decimal)snap.BestBuyQuantity;
            var askDepth = (decimal)snap.BestSellQuantity;

            var depthBull = askDepth > 0m
                ? Math.Min(1m, (bidDepth / askDepth) / _config.OrderBookDepthRatio)
                : 1m;
            var depthBear = bidDepth > 0m
                ? Math.Min(1m, (askDepth / bidDepth) / _config.OrderBookDepthRatio)
                : 1m;

            orderBookBull = (buyRatioScore + depthBull) / 2m;
            orderBookBear = (sellRatioScore + depthBear) / 2m;
        }

        // ─── Intra-Candle check ───
        SubCandleAnalysisResult? subAnalysis = null;
        bool intraCandleInWindow = false;

        if (_config.EnableIntraCandleDecision
            && input.TimeFrame >= TimeFrame.M15
            && input.TimeFrame < TimeFrame.H4
            && _config.SubCandleTimeFrame < input.TimeFrame
            && input.ExtraTimeFrames?.TryGetValue(_config.SubCandleTimeFrame, out var subCandles) == true)
        {
            var currentPrimaryCandleStart = input.Candles[^1].OpenTime + tfDuration;
            var elapsed = input.EvaluatedAt - currentPrimaryCandleStart;

            if (elapsed > TimeSpan.Zero && elapsed < tfDuration / 2)
            {
                intraCandleInWindow = true;

                subAnalysis = SubCandleAnalyzer.Analyze(
                    subCandles,
                    _config.SubCandleTimeFrame.ToDuration(),
                    currentPrimaryCandleStart,
                    tfDuration,
                    input.EvaluatedAt,
                    _config.OpeningBaselineDays,
                    _config.IntraDayBaselineCandles,
                    _config.MinBaselineSamples,
                    _config.IntraCandleVolumeThreshold);

                if (subAnalysis is null || !subAnalysis.Triggered)
                {
                    // Not enough intra-candle evidence; hold until close.
                    return StrategyEvaluationResult.Empty(BuildFeatures(
                        trendBullish, trendBearish,
                        momBullish, momBearish,
                        volumeScore, volumeScore,
                        bullStrength: 0m, bearStrength: 0m,
                        volumeRatio, rsi, currentHist,
                        input.LatestSnapshot?.BuyRatio,
                        intraCandleInWindow: true,
                        subAnalysis: subAnalysis));
                }

                // Override volumeScore with intra-candle evidence
                volumeScore = Math.Min(1m,
                    subAnalysis.VolumeRatio / (_config.IntraCandleVolumeThreshold * 2m));
            }
        }

        var microBullish = orderBookBull.HasValue
            ? (volumeScore + orderBookBull.Value) / 2m
            : volumeScore;
        var microBearish = orderBookBear.HasValue
            ? (volumeScore + orderBookBear.Value) / 2m
            : volumeScore;

        // ─── 4. Composite Strength ───
        var bullish = _config.TrendWeight * trendBullish
                    + _config.MomentumWeight * momBullish
                    + _config.MicroWeight * microBullish;

        var bearish = _config.TrendWeight * trendBearish
                    + _config.MomentumWeight * momBearish
                    + _config.MicroWeight * microBearish;

        
        var diff = bullish - bearish;

        var features = BuildFeatures(
            trendBullish, trendBearish,
            momBullish, momBearish,
            microBullish, microBearish,
            bullish, bearish,
            volumeRatio, rsi, currentHist,
            input.LatestSnapshot?.BuyRatio,
            intraCandleInWindow, subAnalysis);

        // ─── 5. Decision ───
        if (bullish >= _config.MinSignalStrength && diff >= _config.MinStrengthDiff)
        {
            var entryPrice = input.LatestSnapshot is { LastTradedPrice: > 0 } s
                ? (decimal)s.LastTradedPrice
                : input.Candles[^1].Close;

            var atr = _atr.Compute(
                new IndicatorInput(input.Candles, input.LatestSnapshot!, input.TimeFrame),
                new AtrConfig { Period = _config.AtrPeriod });

            Price? sl = null;
            Price? tp = null;

            if (atr.HasValue && atr.Value > 0m)
            {
                var slValue = entryPrice - _config.AtrStopMultiplier * atr.Value;
                var tpValue = entryPrice + _config.AtrTakeProfitMultiplier * atr.Value;

                if (slValue > 0m)
                {
                    sl = new Price(slValue);
                    tp = new Price(tpValue);
                }
            }

            var sig = new StrategySignal(
                Isin: input.Isin,
                Action: SignalAction.Buy,
                Strength: new SignalStrength(bullish),
                MetadataJson: null,
                StopLoss: sl,
                TakeProfit: tp);

            return new StrategyEvaluationResult(new[] { sig }, features);
        }

        if (bearish >= _config.MinSignalStrength && -diff >= _config.MinStrengthDiff)
        {
            // Sell، Position رو می‌بنده — SL/TP معنی نداره
            var sig = new StrategySignal(
                Isin: input.Isin,
                Action: SignalAction.Sell,
                Strength: new SignalStrength(bearish));

            return new StrategyEvaluationResult(new[] { sig }, features);
        }

        return StrategyEvaluationResult.Empty(features);
    }

    /* ═══════════════════ Helpers ═══════════════════ */

    private (decimal IntraDayBaseline, decimal OpeningBaseline, bool IsOpening)?
        ComputeVolumeBaselines(IReadOnlyList<Candle> candles, TimeSpan tfDuration)
    {
        if (candles.Count < 2) return null;

        var intraDay = new List<decimal>(candles.Count);
        var opening = new List<decimal>(candles.Count);

        DateTime? prevOpen = null;
        for (int i = 0; i < candles.Count - 1; i++) // exclude last (current)
        {
            var c = candles[i];
            var gap = prevOpen.HasValue
                ? c.OpenTime - prevOpen.Value
                : TimeSpan.MaxValue;
            var isOpening = gap > TimeSpan.FromHours(6);
            prevOpen = c.OpenTime;

            if (isOpening) opening.Add(c.Volume);
            else intraDay.Add(c.Volume);
        }

        var intraDayTrimmed = intraDay
            .TakeLast(Math.Max(1, _config.IntraDayBaselineCandles)).ToList();
        var openingTrimmed = opening
            .TakeLast(Math.Max(1, _config.OpeningBaselineDays)).ToList();

        if (intraDayTrimmed.Count < _config.MinBaselineSamples
            && openingTrimmed.Count < _config.MinBaselineSamples)
        {
            return null;
        }

        var intraDayBaseline = intraDayTrimmed.Count >= _config.MinBaselineSamples
            ? Median(intraDayTrimmed) : 0m;
        var openingBaseline = openingTrimmed.Count >= _config.MinBaselineSamples
            ? Median(openingTrimmed) : 0m;

        // Is current (last) candle opening?
        var currentGap = prevOpen.HasValue
            ? candles[^1].OpenTime - prevOpen.Value
            : TimeSpan.MaxValue;
        var currentIsOpening = currentGap > TimeSpan.FromHours(6);

        return (intraDayBaseline, openingBaseline, currentIsOpening);
    }

    private static decimal Median(List<decimal> values)
    {
        if (values.Count == 0) return 0m;
        var sorted = values.OrderBy(x => x).ToList();
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2m
            : sorted[mid];
    }

    private static void ValidateWeights(MtfCompositeStrategyConfig config)
    {
        var sum = config.TrendWeight + config.MomentumWeight + config.MicroWeight;
        if (Math.Abs(sum - 1.0m) > 0.001m)
        {
            throw new InvalidOperationException(
                $"MtfComposite weights must sum to 1.0, got {sum}. " +
                $"TrendWeight={config.TrendWeight}, " +
                $"MomentumWeight={config.MomentumWeight}, " +
                $"MicroWeight={config.MicroWeight}.");
        }
    }

    private static IReadOnlyDictionary<string, object?> BuildFeatures(
        decimal trendBull, decimal trendBear,
        decimal momBull, decimal momBear,
        decimal microBull, decimal microBear,
        decimal bullStrength, decimal bearStrength,
        decimal volumeRatio, decimal? rsi, decimal macdHist,
        decimal? buyRatio, bool intraCandleInWindow,
        SubCandleAnalysisResult? subAnalysis)
    {
        return new Dictionary<string, object?>
        {
            ["trendBullish"] = trendBull,
            ["trendBearish"] = trendBear,
            ["momentumBullish"] = momBull,
            ["momentumBearish"] = momBear,
            ["microBullish"] = microBull,
            ["microBearish"] = microBear,
            ["bullishStrength"] = bullStrength,
            ["bearishStrength"] = bearStrength,
            ["diff"] = bullStrength - bearStrength,
            ["volumeRatio"] = volumeRatio,
            ["rsi"] = rsi,
            ["macdHistogram"] = macdHist,
            ["buyRatio"] = buyRatio,
            ["intraCandleInWindow"] = intraCandleInWindow,
            ["intraCandleTriggered"] = subAnalysis?.Triggered,
            ["subVolumeRatio"] = subAnalysis?.VolumeRatio,
        };
    }
}