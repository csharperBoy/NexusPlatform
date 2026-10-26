using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

/// <summary>
/// یک رویداد در چرخه‌ی Trading Run. هر رویداد شامل:
/// - Correlation: به Run/Isin/Signal/Order/Position (هر کدام موجود بود)
/// - نوع و سطح
/// - پیام فارسی برای کاربر
/// - Payload فنی (JSON) برای Debug
/// </summary>
public class TradingEvent : BaseEntity
{
    // ─── Correlation ───
    public Guid? RunId { get; private set; }
    public string? Isin { get; private set; }
    public Guid? AccountId { get; private set; }
    public Guid? SignalId { get; private set; }
    public Guid? OrderIntentId { get; private set; }
    public Guid? BrokerOrderId { get; private set; }
    public Guid? TradeId { get; private set; }
    public Guid? PositionId { get; private set; }

    // ─── Event ───
    public TradingEventType Type { get; private set; }
    public TradingEventSeverity Severity { get; private set; }
    public TradingStage Stage { get; private set; }
    public DateTime OccurredAt { get; private set; }

    // ─── Human-readable (Persian) ───
    public string Message { get; private set; } = default!;

    // ─── Technical details (JSON) ───
    public string? PayloadJson { get; private set; }

    private TradingEvent() { } // EF

    public static TradingEvent Create(
        TradingStage stage,
        TradingEventType type,
        TradingEventSeverity severity,
        string message,
        string? payloadJson = null,
        Guid? runId = null,
        string? isin = null,
        Guid? accountId = null,
        Guid? signalId = null,
        Guid? orderIntentId = null,
        Guid? brokerOrderId = null,
        Guid? tradeId = null,
        Guid? positionId = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));

        return new TradingEvent
        {
            Stage = stage,
            Type = type,
            Severity = severity,
            Message = message,
            PayloadJson = payloadJson,
            RunId = runId,
            Isin = isin,
            AccountId = accountId,
            SignalId = signalId,
            OrderIntentId = orderIntentId,
            BrokerOrderId = brokerOrderId,
            TradeId = tradeId,
            PositionId = positionId,
            OccurredAt = DateTime.UtcNow
        };
    }
}