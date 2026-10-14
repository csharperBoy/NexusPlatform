using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.TradingRuns;

public sealed class TradingRunByIdWithChildrenSpec : BaseSpecification<TradingRun>
{
    public TradingRunByIdWithChildrenSpec(Guid id)
        : base(r => r.Id == id)
    {
        AddInclude(r => r.Symbols);
        AddInclude(r => r.Accounts);
    }
}