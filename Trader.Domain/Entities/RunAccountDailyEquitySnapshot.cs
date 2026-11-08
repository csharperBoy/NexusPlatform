using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>
/// Opening Equity یک (Run, Account) در یک روز معاملاتی (تهران).
/// Unique: (RunId, AccountId, TradingDate).
/// Immutable — Day reset → Record جدید.
/// </summary>
public class RunAccountDailyEquitySnapshot : BaseEntity, IAuditableEntity
{
    public Guid RunId { get; private set; }
    public Guid AccountId { get; private set; }
    public DateOnly TradingDate { get; private set; }

    public decimal OpeningCash { get; private set; }
    public decimal OpeningPositionValue { get; private set; }
    public decimal OpeningEquity { get; private set; }

    public DateTime CapturedAt { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public void Touch() => ModifiedAt = DateTime.UtcNow;

    private RunAccountDailyEquitySnapshot() { } // EF

    public static RunAccountDailyEquitySnapshot Create(
        Guid runId,
        Guid accountId,
        DateOnly tradingDate,
        decimal openingCash,
        decimal openingPositionValue)
    {
        if (runId == Guid.Empty)
            throw new ArgumentException("RunId required.", nameof(runId));
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId required.", nameof(accountId));
        if (openingCash < 0m)
            throw new ArgumentOutOfRangeException(nameof(openingCash));
        if (openingPositionValue < 0m)
            throw new ArgumentOutOfRangeException(nameof(openingPositionValue));

        return new RunAccountDailyEquitySnapshot
        {
            RunId = runId,
            AccountId = accountId,
            TradingDate = tradingDate,
            OpeningCash = openingCash,
            OpeningPositionValue = openingPositionValue,
            OpeningEquity = openingCash + openingPositionValue,
            CapturedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        };
    }
}