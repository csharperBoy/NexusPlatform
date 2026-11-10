using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.PriceCollection;

public sealed class PriceSourceByCodeSpec : BaseSpecification<PriceSource>
{
    public PriceSourceByCodeSpec(string code)
        : base(s => s.Code == code)
    {
    }
}