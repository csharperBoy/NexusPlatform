using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Risk;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Risk.Guards;

public sealed class DailyLossLimitGuard : IRiskGuard
{
    private readonly IRunDailyEquityTracker _tracker;
    private readonly IOptionsMonitor<TradingRiskOptions> _options;

    public DailyLossLimitGuard(
        IRunDailyEquityTracker tracker,
        IOptionsMonitor<TradingRiskOptions> options)
    {
        _tracker = tracker;
        _options = options;
    }

    public string Name => "DailyLossLimit";

    public async Task<RiskDecision> CheckAsync(
        RiskCheckContext context, CancellationToken ct = default)
    {
        var opts = _options.CurrentValue.DailyLossLimit;

        if (!opts.Enabled)
            return RiskDecision.Allow;

        if (context.RunContext.ExecutionMode == ExecutionMode.Paper && !opts.ApplyToPaper)
            return RiskDecision.Allow;

        // ── per-Account check ──
        var current = await _tracker.GetCurrentAsync(
            context.RunContext.RunId,
            context.Account.Id,
            ct);

        if (current.LossPercent >= opts.MaxLossPercent)
        {
            return RiskDecision.Block(
                $"Daily loss {current.LossPercent:F2}% for account '{context.Account.Name}' " +
                $"exceeds limit {opts.MaxLossPercent:F2}% " +
                $"(opening={current.Opening.OpeningEquity:N0}, " +
                $"current={current.CurrentEquity:N0})",
                Name);
        }

        return RiskDecision.Allow;
    }
}