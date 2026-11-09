using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Risk;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Risk.Guards;

public sealed class RateLimiterGuard : IRiskGuard
{
    private readonly IRateLimiterTracker _tracker;
    private readonly IOptionsMonitor<TradingRiskOptions> _options;

    public RateLimiterGuard(
        IRateLimiterTracker tracker,
        IOptionsMonitor<TradingRiskOptions> options)
    {
        _tracker = tracker;
        _options = options;
    }

    public string Name => "RateLimiter";

    public Task<RiskDecision> CheckAsync(
        RiskCheckContext context, CancellationToken ct = default)
    {
        var opts = _options.CurrentValue.RateLimiter;
        if (!opts.Enabled)
            return Task.FromResult(RiskDecision.Allow);

        if (context.RunContext.ExecutionMode == ExecutionMode.Paper && !opts.ApplyToPaper)
            return Task.FromResult(RiskDecision.Allow);

        var window = TimeSpan.FromSeconds(opts.WindowSeconds);
        var count = _tracker.GetCountInWindow(context.Account.Id, window);

        if (count >= opts.OrdersPerMinutePerAccount)
        {
            return Task.FromResult(RiskDecision.Block(
                $"Rate limit reached for account '{context.Account.Name}': " +
                $"{count}/{opts.OrdersPerMinutePerAccount} orders in " +
                $"last {opts.WindowSeconds}s.",
                Name));
        }

        return Task.FromResult(RiskDecision.Allow);
    }
}