using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;

namespace Trader.Infrastructure.Health;

public sealed class MarketDataFreshnessHealthCheck : IHealthCheck
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly IMarketSessionProvider _session;
    private readonly ITradingScheduleCache _scheduleCache;
    private readonly IOptionsMonitor<TradingRuntimeOptions> _options;

    public MarketDataFreshnessHealthCheck(
        IRunExecutionContextCache runCache,
        IMarketSnapshotStore snapshotStore,
        IMarketSessionProvider session,
        ITradingScheduleCache scheduleCache,
        IOptionsMonitor<TradingRuntimeOptions> options)
    {
        _runCache = runCache;
        _snapshotStore = snapshotStore;
        _session = session;
        _scheduleCache = scheduleCache;
        _options = options;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var runs = _runCache.GetAllActive();
        if (runs.Count == 0)
        {
            return HealthCheckResult.Healthy(
                "No active runs.",
                data: new Dictionary<string, object> { ["activeRuns"] = 0 });
        }

        var now = DateTime.UtcNow;
        var heartbeat = _options.CurrentValue.HeartbeatInterval;
        var threshold = heartbeat * 3;   // 3 × heartbeat = 30s default

        var checkedSymbols = 0;
        var skippedClosed = 0;
        var stale = new List<string>();
        var missing = new List<string>();

        foreach (var run in runs)
        {
            foreach (var sym in run.Symbols)
            {
                var schedule = await _scheduleCache.GetForIsinAsync(
                    sym.Isin, cancellationToken);

                if (!_session.IsMarketOpen(schedule, now))
                {
                    skippedClosed++;
                    continue;
                }

                checkedSymbols++;

                var snap = _snapshotStore.GetLatest(sym.Isin);
                if (snap is null)
                {
                    missing.Add(sym.Isin);
                    continue;
                }

                var age = now - snap.CapturedAt;
                if (age > threshold)
                {
                    stale.Add($"{sym.Isin} ({age.TotalSeconds:F0}s old)");
                }
            }
        }

        var data = new Dictionary<string, object>
        {
            ["activeRuns"] = runs.Count,
            ["checkedSymbols"] = checkedSymbols,
            ["skippedClosedMarkets"] = skippedClosed,
            ["thresholdSeconds"] = Math.Round(threshold.TotalSeconds, 2),
        };

        if (stale.Count > 0) data["stale"] = stale.ToArray();
        if (missing.Count > 0) data["missing"] = missing.ToArray();

        if (stale.Count == 0 && missing.Count == 0)
        {
            return HealthCheckResult.Healthy(
                $"All {checkedSymbols} active symbols have fresh market data.",
                data: data);
        }

        return HealthCheckResult.Degraded(
            $"Market data issues: {stale.Count} stale, {missing.Count} missing " +
            $"(of {checkedSymbols} active symbols).",
            data: data);
    }
}