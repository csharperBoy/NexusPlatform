using Microsoft.Extensions.Diagnostics.HealthChecks;
using Trader.Application.Abstractions.Health;

namespace Trader.Infrastructure.Health;

public sealed class HistoricalWorkerHealthCheck : IHealthCheck
{
    // Matches HistoricalDataCollectionWorker.PollInterval (15s)
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan Threshold = PollInterval * 3;

    private readonly IRuntimeHealthState _state;

    public HistoricalWorkerHealthCheck(IRuntimeHealthState state)
    {
        _state = state;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var lastPoll = _state.LastHistoricalPollAt;

        if (lastPoll is null)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                "Historical data worker has not polled yet."));
        }

        var age = now - lastPoll.Value;
        var data = new Dictionary<string, object>
        {
            ["lastPollAt"] = lastPoll.Value.ToString("o"),
            ["ageSeconds"] = Math.Round(age.TotalSeconds, 2),
            ["thresholdSeconds"] = Math.Round(Threshold.TotalSeconds, 2),
        };

        if (age > Threshold)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                $"Historical data worker stalled. Last poll {age.TotalSeconds:F1}s ago.",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            $"Last poll {age.TotalSeconds:F1}s ago.",
            data: data));
    }
}