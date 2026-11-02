using Core.Domain.Common.EntityProperties;

namespace Trader.Domain.Entities;

public class BacktestSymbol : BaseEntity
{
    public Guid BacktestRunId { get; private set; }
    public string Isin { get; private set; } = default!;
    public decimal AllocationPercentage { get; private set; }

    private BacktestSymbol() { } // EF

    internal static BacktestSymbol Create(
        Guid backtestRunId, string isin, decimal allocationPercentage)
    {
        return new BacktestSymbol
        {
            BacktestRunId = backtestRunId,
            Isin = isin.Trim(),
            AllocationPercentage = allocationPercentage
        };
    }
}