using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.MarketData;

public sealed class CurrencyCandleTransformer : ICandleTransformer
{
    private const string BaseCurrency = "IRR";

    private readonly ICurrencyRateProvider _rateProvider;
    private readonly ILogger<CurrencyCandleTransformer> _logger;

    public CurrencyCandleTransformer(
        ICurrencyRateProvider rateProvider,
        ILogger<CurrencyCandleTransformer> logger)
    {
        _rateProvider = rateProvider;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Candle>> TransformAsync(
        IReadOnlyList<Candle> irrCandles,
        string targetCurrency,
        DateTime atTime,
        CancellationToken ct = default)
    {
        if (irrCandles.Count == 0) return irrCandles;

        var target = targetCurrency.Trim().ToUpperInvariant();
        if (target == BaseCurrency) return irrCandles;

        // ─── یک نرخ برای همه‌ی Candleها (نرخ in EffectAt=atTime) ───
        // Rationale: تحلیل «الان به دلار» است، نه «اگر هر روز با نرخ آن روز
        // تبدیل می‌شد». برای Backtesting آینده یک مسیر دیگر اضافه می‌کنیم.
        var rate = await _rateProvider.GetRateAsync(
            BaseCurrency, target, atTime, ct);

        if (rate <= 0m)
            throw new InvalidOperationException(
                $"Rate {BaseCurrency}→{target} is invalid: {rate}.");

        // rate = "چند تا target برای ۱ IRR"
        // قیمت IRR → target: irr_price * rate
        var result = new List<Candle>(irrCandles.Count);
        foreach (var c in irrCandles)
        {
            result.Add(c with
            {
                Open = c.Open * rate,
                High = c.High * rate,
                Low = c.Low * rate,
                Close = c.Close * rate,
                // Volume دست‌نخورده — تعداد سهم
            });
        }

        _logger.LogDebug(
            "Transformed {Count} candles: IRR→{Target} rate={Rate:E6}",
            result.Count, target, rate);

        return result;
    }
}