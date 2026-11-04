using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.Health;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Health;

public sealed class RuntimeWorkerHealthCheck : IHealthCheck
{
    private readonly IRuntimeHealthState _state;
    private readonly IOptionsMonitor<TradingRuntimeOptions> _options;

    public RuntimeWorkerHealthCheck(
        IRuntimeHealthState state,
        IOptionsMonitor<TradingRuntimeOptions> options)
    {
        _state = state;
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var lastTick = _state.LastTickAt;

        if (lastTick is null)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                "Runtime worker has not produced a tick yet.",
                data: new Dictionary<string, object>
                {
                    ["lastTickAt"] = "null",
                }));
        }

        var interval = _options.CurrentValue.HeartbeatInterval;
        var threshold = interval * 2;
        var age = now - lastTick.Value;

        var data = new Dictionary<string, object>
        {
            ["lastTickAt"] = lastTick.Value.ToString("o"),
            ["ageSeconds"] = Math.Round(age.TotalSeconds, 2),
            ["thresholdSeconds"] = Math.Round(threshold.TotalSeconds, 2),
        };

        if (age > threshold)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Runtime worker stalled. Last tick {age.TotalSeconds:F1}s ago " +
                $"(threshold {threshold.TotalSeconds:F1}s).",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Last tick {age.TotalSeconds:F1}s ago.",
            data: data));
    }
}