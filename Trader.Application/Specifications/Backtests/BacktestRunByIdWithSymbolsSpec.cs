using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Backtests;

public sealed class BacktestRunByIdWithSymbolsSpec : BaseSpecification<BacktestRun>
{
    public BacktestRunByIdWithSymbolsSpec(Guid id)
        : base(b => b.Id == id)
    {
        AddInclude(b => b.Symbols);
    }
}