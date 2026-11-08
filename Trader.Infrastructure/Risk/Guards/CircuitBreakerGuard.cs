using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Risk;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Risk.Guards;

public sealed class CircuitBreakerGuard : IRiskGuard
{
    private readonly IRunAccountLossStreakTracker _tracker;
    private readonly IOptionsMonitor<TradingRiskOptions> _options;

    public CircuitBreakerGuard(
        IRunAccountLossStreakTracker tracker,
        IOptionsMonitor<TradingRiskOptions> options)
    {
        _tracker = tracker;
        _options = options;
    }

    public string Name => "CircuitBreaker";

    public async Task<RiskDecision> CheckAsync(
        RiskCheckContext context, CancellationToken ct = default)
    {
        var opts = _options.CurrentValue.CircuitBreaker;
        if (!opts.Enabled) return RiskDecision.Allow;

        if (context.RunContext.ExecutionMode == ExecutionMode.Paper && !opts.ApplyToPaper)
            return RiskDecision.Allow;

        var state = await _tracker.GetAsync(
            context.RunContext.RunId, context.Account.Id, ct);

        if (state.IsCircuitActive(DateTime.UtcNow))
        {
            var remainingMin = (int)(state.CooldownExpiresAt!.Value - DateTime.UtcNow)
                .TotalMinutes;

            return RiskDecision.Block(
                $"Circuit breaker active for account '{context.Account.Name}': " +
                $"{state.ConsecutiveLosses} consecutive losses, " +
                $"cooldown expires in {remainingMin} min " +
                $"(at {state.CooldownExpiresAt:O}).",
                Name);
        }

        return RiskDecision.Allow;
    }
}