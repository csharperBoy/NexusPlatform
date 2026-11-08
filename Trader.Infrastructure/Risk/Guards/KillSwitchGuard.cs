using Trader.Application.Abstractions.Risk;

namespace Trader.Infrastructure.Risk.Guards;

public sealed class KillSwitchGuard : IRiskGuard
{
    private readonly IRiskStateTracker _state;

    public KillSwitchGuard(IRiskStateTracker state)
    {
        _state = state;
    }

    public string Name => "KillSwitch";

    public Task<RiskDecision> CheckAsync(
        RiskCheckContext context, CancellationToken ct = default)
    {
        if (_state.IsGlobalKillSwitchActive)
        {
            return Task.FromResult(RiskDecision.Block(
                "Global Kill Switch is active.",
                Name));
        }

        if (_state.IsAccountKillSwitchActive(context.Account.Id))
        {
            return Task.FromResult(RiskDecision.Block(
                $"Kill Switch active for account '{context.Account.Name}' ({context.Account.Id}).",
                Name));
        }

        return Task.FromResult(RiskDecision.Allow);
    }
}