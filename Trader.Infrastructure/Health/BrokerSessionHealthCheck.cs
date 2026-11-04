using Microsoft.Extensions.Diagnostics.HealthChecks;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Health;

public sealed class BrokerSessionHealthCheck : IHealthCheck
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;

    public BrokerSessionHealthCheck(
        IRunExecutionContextCache runCache,
        ITraderUnitOfWork<TraderDbContext> uow)
    {
        _runCache = runCache;
        _uow = uow;
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

        var accountIds = runs
            .SelectMany(r => r.AccountIds)
            .Distinct()
            .ToList();

        var now = DateTime.UtcNow;
        var missingSession = new List<string>();
        var expiredSession = new List<string>();

        foreach (var accountId in accountIds)
        {
            var account = await _uow.Accounts.GetByIdAsync(accountId);
            if (account is null)
            {
                missingSession.Add($"{accountId} (not found)");
                continue;
            }

            var hasSession = !string.IsNullOrWhiteSpace(account.EncryptedSession);
            var isExpired = account.SessionExp.HasValue
                            && account.SessionExp.Value <= now;

            if (!hasSession)
                missingSession.Add($"{account.Name} ({accountId})");
            else if (isExpired)
                expiredSession.Add($"{account.Name} ({accountId})");
        }

        var data = new Dictionary<string, object>
        {
            ["accountsChecked"] = accountIds.Count,
            ["missingCount"] = missingSession.Count,
            ["expiredCount"] = expiredSession.Count,
        };

        if (missingSession.Count > 0)
            data["missing"] = missingSession.ToArray();

        if (expiredSession.Count > 0)
            data["expired"] = expiredSession.ToArray();

        if (missingSession.Count == 0 && expiredSession.Count == 0)
        {
            return HealthCheckResult.Healthy(
                $"All {accountIds.Count} accounts have valid sessions.",
                data: data);
        }

        return HealthCheckResult.Degraded(
            $"Session issues: {missingSession.Count} missing, " +
            $"{expiredSession.Count} expired (of {accountIds.Count}). " +
            $"Auto-relogin may resolve on next tick.",
            data: data);
    }
}