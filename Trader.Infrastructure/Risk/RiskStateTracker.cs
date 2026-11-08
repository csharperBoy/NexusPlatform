using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Specifications.Risk;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Risk;

/// <summary>
/// Singleton — Cache فعال Kill Switchها در حافظه.
/// On write: persist به DB + refresh cache.
/// On startup: lazy-load از DB در اولین read.
/// </summary>
public sealed class RiskStateTracker : IRiskStateTracker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RiskStateTracker> _logger;

    // ─── State (thread-safe) ───
    private sealed record KillSwitchState(bool Global, IReadOnlySet<Guid> Accounts);

    private KillSwitchState _state = new(false, new HashSet<Guid>());
    private volatile bool _loaded;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public RiskStateTracker(
        IServiceScopeFactory scopeFactory,
        ILogger<RiskStateTracker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }
    public IReadOnlySet<Guid> ActiveAccountKillSwitches => _state.Accounts;
    public bool IsGlobalKillSwitchActive => _state.Global;

    public bool IsAccountKillSwitchActive(Guid accountId)
        => _state.Accounts.Contains(accountId);

    public async Task ActivateKillSwitchAsync(
        Guid? accountId, string? activatedBy, string? reason,
        CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        // ── Guard: قبلاً فعال نبوده ──
        var existing = (await uow.RiskKillSwitchSpecs.ListBySpecAsync(
            new ActiveKillSwitchesSpec()))
            .FirstOrDefault(x => x.AccountId == accountId);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Kill switch already active for {Scope}.",
                accountId is null ? "Global" : $"Account {accountId}");
            return;
        }

        var entity = Domain.Entities.RiskKillSwitch.Activate(accountId, activatedBy, reason);
        await uow.RiskKillSwitches.AddAsync(entity);
        await uow.SaveChangesAsync(ct);

        await RefreshAsync(ct);

        _logger.LogWarning(
            "🚨 Kill Switch ACTIVATED — Scope: {Scope}, By: {By}, Reason: {Reason}",
            accountId is null ? "Global" : $"Account {accountId}",
            activatedBy ?? "system",
            reason ?? "—");
    }

    public async Task DeactivateKillSwitchAsync(
        Guid? accountId, string? deactivatedBy,
        CancellationToken ct = default)
    {
        await EnsureLoadedAsync(ct);

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var existing = (await uow.RiskKillSwitchSpecs.ListBySpecAsync(
            new ActiveKillSwitchesSpec()))
            .FirstOrDefault(x => x.AccountId == accountId);

        if (existing is null)
        {
            _logger.LogInformation(
                "No active kill switch to deactivate for {Scope}.",
                accountId is null ? "Global" : $"Account {accountId}");
            return;
        }

        existing.Deactivate(deactivatedBy);
        await uow.RiskKillSwitches.UpdateAsync(existing);
        await uow.SaveChangesAsync(ct);

        await RefreshAsync(ct);

        _logger.LogWarning(
            "✅ Kill Switch DEACTIVATED — Scope: {Scope}, By: {By}",
            accountId is null ? "Global" : $"Account {accountId}",
            deactivatedBy ?? "system");
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var active = (await uow.RiskKillSwitchSpecs.ListBySpecAsync(
            new ActiveKillSwitchesSpec())).ToList();

        var newGlobal = active.Any(x => x.AccountId is null);
        var newAccounts = active
            .Where(x => x.AccountId.HasValue)
            .Select(x => x.AccountId!.Value)
            .ToHashSet();

        Interlocked.Exchange(ref _state, new KillSwitchState(newGlobal, newAccounts));
        _loaded = true;

        _logger.LogDebug(
            "Risk state refreshed: Global={Global}, ActiveAccounts={Count}",
            newGlobal, newAccounts.Count);
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_loaded) return;

        await _loadLock.WaitAsync(ct);
        try
        {
            if (_loaded) return;
            await RefreshAsync(ct);
        }
        finally
        {
            _loadLock.Release();
        }
    }
}