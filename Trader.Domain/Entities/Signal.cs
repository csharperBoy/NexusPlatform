using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

public class Signal : BaseEntity
{
    public Guid RunId { get; private set; }
    public string Isin { get; private set; } = default!;
    public SignalAction Action { get; private set; }
    public SignalStrength Strength { get; private set; }
    public string StrategyKey { get; private set; } = default!;
    public DateTime ProducedAt { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public Guid? SourceEvaluationId { get; private set; }

    /// <summary>پیشنهاد Strategy برای SL. اگر null بود، Run-level اعمال می‌شه.</summary>
    public Price? StopLossPrice { get; private set; }

    /// <summary>پیشنهاد Strategy برای TP. اگر null بود، Run-level اعمال می‌شه.</summary>
    public Price? TakeProfitPrice { get; private set; }

    private Signal() { } // EF

    public static Signal Create(
        Guid runId,
        string isin,
        string strategyKey,
        SignalAction action,
        SignalStrength strength,
        string? metadataJson = null,
        Guid? sourceEvaluationId = null,
        Price? stopLossPrice = null,
        Price? takeProfitPrice = null)
    {
        return new Signal
        {
            RunId = runId,
            Isin = isin,
            StrategyKey = strategyKey,
            Action = action,
            Strength = strength,
            ProducedAt = DateTime.UtcNow,
            MetadataJson = metadataJson,
            SourceEvaluationId = sourceEvaluationId,
            StopLossPrice = stopLossPrice,
            TakeProfitPrice = takeProfitPrice,
        };
    }

    public void MarkConsumed()
    {
        ConsumedAt = DateTime.UtcNow;
    }
}