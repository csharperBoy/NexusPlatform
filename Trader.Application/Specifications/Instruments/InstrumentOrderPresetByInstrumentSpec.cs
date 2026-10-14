using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Instruments;

public sealed class InstrumentOrderPresetByInstrumentSpec : BaseSpecification<InstrumentOrderPreset>
{
    public InstrumentOrderPresetByInstrumentSpec(Guid instrumentId)
        : base(p => p.InstrumentId == instrumentId)
    {
    }
}