using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Risk;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Risk.Guards;

/// <summary>
/// اگر Run در یک روز بیش از MaxLossPercent ضرر کرده باشد، Orderها Block می‌شوند.
///
/// Run-level، Block-only (Positionها با SL/TP محافظت می‌شوند).
/// Paper Mode skip (به‌جز اگر ApplyToPaper = true).
/// </summary>
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

        // Paper Mode skip (default) — Paper Cash tracked نیست
        if (context.RunContext.ExecutionMode == ExecutionMode.Paper && !opts.ApplyToPaper)
            return RiskDecision.Allow;

        // ممکن است throw کند (Broker down) → pipeline به Block تبدیل می‌کند
        var current = await _tracker.GetCurrentAsync(
            context.RunContext.RunId, ct);

        if (current.LossPercent >= opts.MaxLossPercent)
        {
            return RiskDecision.Block(
                $"Daily loss {current.LossPercent:F2}% " +
                $"exceeds limit {opts.MaxLossPercent:F2}% " +
                $"(opening={current.Opening.OpeningEquity:N0}, " +
                $"current={current.CurrentEquity:N0})",
                Name);
        }

        return RiskDecision.Allow;
    }
}