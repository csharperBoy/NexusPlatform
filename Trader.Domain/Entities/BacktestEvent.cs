using Core.Domain.Common.EntityProperties;
using Trader.Domain.Enums;

namespace Trader.Domain.Entities;

public class BacktestEvent : BaseEntity
{
    public Guid BacktestRunId { get; private set; }
    public BacktestEventType Type { get; private set; }
    public BacktestEventSeverity Severity { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string Message { get; private set; } = default!;
    public string? PayloadJson { get; private set; }

    // Correlation
    public string? Isin { get; private set; }
    public DateTime? CandleTime { get; private set; }
    public Guid? TradeId { get; private set; }

    private BacktestEvent() { } // EF

    public static BacktestEvent Create(
        Guid backtestRunId,
        BacktestEventType type,
        BacktestEventSeverity severity,
        string message,
        string? payloadJson = null,
        string? isin = null,
        DateTime? candleTime = null,
        Guid? tradeId = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));

        return new BacktestEvent
        {
            BacktestRunId = backtestRunId,
            Type = type,
            Severity = severity,
            OccurredAt = DateTime.UtcNow,
            Message = message,
            PayloadJson = payloadJson,
            Isin = isin,
            CandleTime = candleTime,
            TradeId = tradeId
        };
    }
}