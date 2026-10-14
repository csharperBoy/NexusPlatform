using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Instruments;

public sealed class ActiveInstrumentsSpec : BaseSpecification<Instrument>
{
    public ActiveInstrumentsSpec()
        : base(i => i.IsActive)
    {
        ApplyOrderBy(i => i.SymbolName);
    }
}