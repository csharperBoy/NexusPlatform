using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

/// <summary>
/// ثبت هر بار Evaluate شدن یک Strategy برای یک Symbol در یک Run.
/// برای AI/ML Training و Debug استفاده می‌شود.
/// </summary>
public class StrategyEvaluation : BaseEntity
{
    public Guid RunId { get; private set; }
    public string Isin { get; private set; } = default!;
    public string StrategyKey { get; private set; } = default!;
    public DateTime EvaluatedAt { get; private set; }

    /// <summary>قیمت Snapshot در لحظه‌ی Evaluate (ریال).</summary>
    public decimal PriceAtEval { get; private set; }

    /// <summary>Feature Vector به صورت JSON (rsi, period, macd, ...).</summary>
    public string FeaturesJson { get; private set; } = "{}";

    /// <summary>تصمیم: 0=Hold, 1=Buy, 2=Sell.</summary>
    public SignalAction Action { get; private set; }

    /// <summary>قدرت تصمیم [0..1].</summary>
    public decimal Strength { get; private set; }

    /// <summary>اگر Signal تولید شد، به آن لینک می‌شود.</summary>
    public Guid? SignalId { get; private set; }

    /// <summary>نتیجه‌ی واقعی معامله (بعد از بسته شدن Position پر می‌شود).</summary>
    public string? OutcomeJson { get; private set; }
    public DateTime? OutcomeUpdatedAt { get; private set; }

    private StrategyEvaluation() { } // EF

    public static StrategyEvaluation Create(
        Guid runId,
        string isin,
        string strategyKey,
        DateTime evaluatedAt,
        decimal priceAtEval,
        string featuresJson,
        SignalAction action,
        decimal strength)
    {
        return new StrategyEvaluation
        {
            RunId = runId,
            Isin = isin,
            StrategyKey = strategyKey,
            EvaluatedAt = evaluatedAt,
            PriceAtEval = priceAtEval,
            FeaturesJson = string.IsNullOrWhiteSpace(featuresJson) ? "{}" : featuresJson,
            Action = action,
            Strength = strength
        };
    }

    public void LinkSignal(Guid signalId)
    {
        SignalId = signalId;
    }

    public void UpdateOutcome(string outcomeJson)
    {
        OutcomeJson = outcomeJson;
        OutcomeUpdatedAt = DateTime.UtcNow;
    }
}