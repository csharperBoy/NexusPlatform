using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

/// <summary>اتصال یک TradingAccount به یک TradingRun.</summary>
public class TradingRunAccount : BaseEntity
{
    public Guid TradingRunId { get; private set; }
    public Guid TraderAccountId { get; private set; }

    private TradingRunAccount() { } // EF

    internal static TradingRunAccount Create(Guid runId, Guid accountId)
    {
        return new TradingRunAccount
        {
            TradingRunId = runId,
            TraderAccountId = accountId
        };
    }
}