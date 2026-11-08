using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// Snapshot از Opening Equity یک Run در یک روز معاملاتی (تهران).
/// Lazy capture: در اولین intent روز capture می‌شود (نه در Startup).
///
/// Unique: (RunId, TradingDate).
/// Immutable: پس از Create تغییر نمی‌کند (Day reset → Record جدید).
/// </summary>
public class RunDailyEquitySnapshot : BaseEntity, IAuditableEntity
{
    public Guid RunId { get; private set; }
    public DateOnly TradingDate { get; private set; }

    public decimal OpeningCash { get; private set; }
    public decimal OpeningPositionValue { get; private set; }
    public decimal OpeningEquity { get; private set; }

    public DateTime CapturedAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private RunDailyEquitySnapshot() { } // EF

    public static RunDailyEquitySnapshot Create(
        Guid runId,
        DateOnly tradingDate,
        decimal openingCash,
        decimal openingPositionValue)
    {
        if (runId == Guid.Empty)
            throw new ArgumentException("RunId required.", nameof(runId));
        if (openingCash < 0m)
            throw new ArgumentOutOfRangeException(nameof(openingCash),
                "OpeningCash cannot be negative.");
        if (openingPositionValue < 0m)
            throw new ArgumentOutOfRangeException(nameof(openingPositionValue),
                "OpeningPositionValue cannot be negative.");

        return new RunDailyEquitySnapshot
        {
            RunId = runId,
            TradingDate = tradingDate,
            OpeningCash = openingCash,
            OpeningPositionValue = openingPositionValue,
            OpeningEquity = openingCash + openingPositionValue,
            CapturedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };
    }
}