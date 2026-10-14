using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>
/// خروجی تحلیل یک Strategy برای یک Instrument در یک لحظه.
/// به هیچ Account یا Broker وابسته نیست.
/// </summary>
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

    private Signal() { } // EF

    public static Signal Create(
        Guid runId,
        string isin,
        string strategyKey,
        SignalAction action,
        SignalStrength strength,
        string? metadataJson = null)
    {
        return new Signal
        {
            RunId = runId,
            Isin = isin,
            StrategyKey = strategyKey,
            Action = action,
            Strength = strength,
            ProducedAt = DateTime.UtcNow,
            MetadataJson = metadataJson
        };
    }

    public void MarkConsumed()
    {
        ConsumedAt = DateTime.UtcNow;
    }
}