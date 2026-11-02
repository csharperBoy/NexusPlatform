using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

public class BacktestDailyEquity : BaseEntity
{
    public Guid BacktestRunId { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Equity { get; private set; }
    public decimal Cash { get; private set; }
    public decimal PositionValue { get; private set; }
    public decimal DailyReturnPct { get; private set; }

    private BacktestDailyEquity() { } // EF

    public static BacktestDailyEquity Create(
        Guid backtestRunId,
        DateTime date,
        decimal equity,
        decimal cash,
        decimal positionValue,
        decimal dailyReturnPct)
    {
        return new BacktestDailyEquity
        {
            BacktestRunId = backtestRunId,
            Date = date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc),
            Equity = equity,
            Cash = cash,
            PositionValue = positionValue,
            DailyReturnPct = dailyReturnPct
        };
    }
}