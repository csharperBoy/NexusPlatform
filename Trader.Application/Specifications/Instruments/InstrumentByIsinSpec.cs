using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Instruments;

public sealed class InstrumentByIsinSpec : BaseSpecification<Instrument>
{
    public InstrumentByIsinSpec(string isin)
        : base(i => i.Isin == isin)
    {
    }
}