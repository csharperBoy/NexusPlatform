using Core.Domain.Specifications;
using Trader.Domain.Entities;

namespace Trader.Application.Specifications.Risk;

/// <summary>همه‌ی Kill Switchهای فعال (Global + per-Account).</summary>
public sealed class ActiveKillSwitchesSpec : BaseSpecification<RiskKillSwitch>
{
    public ActiveKillSwitchesSpec()
        : base(x => x.DeactivatedAt == null)
    {
    }
}