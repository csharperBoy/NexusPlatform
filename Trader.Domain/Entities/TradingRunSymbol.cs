using Core.Domain.Common.EntityProperties;
using Trader.Domain.ValueObjects;

namespace Trader.Domain.Entities;

/// <summary>نماد انتخاب‌شده در یک TradingRun + درصد تخصیص سرمایه.</summary>
public class TradingRunSymbol : BaseEntity
{
    public Guid TradingRunId { get; private set; }
    public string Isin { get; private set; } = default!;
    public SymbolAllocation Allocation { get; private set; }

    private TradingRunSymbol() { } // EF

    internal static TradingRunSymbol Create(Guid runId, string isin, SymbolAllocation allocation)
    {
        if (string.IsNullOrWhiteSpace(isin))
            throw new ArgumentException("ISIN is required.", nameof(isin));

        return new TradingRunSymbol
        {
            TradingRunId = runId,
            Isin = isin.Trim(),
            Allocation = allocation
        };
    }

    internal void UpdateAllocation(SymbolAllocation allocation)
    {
        Allocation = allocation;
    }
}