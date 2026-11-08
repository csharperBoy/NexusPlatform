using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Specifications.Risk;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Risk;

public sealed class RunAccountLossStreakTracker : IRunAccountLossStreakTracker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TradingRiskOptions> _options;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<RunAccountLossStreakTracker> _logger;

    private readonly ConcurrentDictionary<(Guid RunId, Guid AccountId), LossStreakState> _cache = new();
    private readonly ConcurrentDictionary<(Guid RunId, Guid AccountId), SemaphoreSlim> _locks = new();

    public RunAccountLossStreakTracker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TradingRiskOptions> options,
        ITradingEventLogger eventLogger,
        ILogger<RunAccountLossStreakTracker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task<LossStreakState> GetAsync(
        Guid runId, Guid accountId, CancellationToken ct = default)
    {
        var key = (runId, accountId);

        if (_cache.TryGetValue(key, out var cached))
        {
            // ── Lazy reset روی Cooldown expiry ──
            if (cached.CircuitBrokenAt.HasValue
                && cached.CooldownExpiresAt.HasValue
                && DateTime.UtcNow >= cached.CooldownExpiresAt.Value)
            {
                await ResetAsync(runId, accountId, "CooldownExpired", ct);
                return cached with
                {
                    ConsecutiveLosses = 0,
                    CircuitBrokenAt = null,
                    CooldownExpiresAt = null,
                };
            }
            return cached;
        }

        // Cache miss → load از DB
        var loaded = await LoadFromDbAsync(runId, accountId, ct);
        if (loaded is not null)
        {
            _cache[key] = loaded;
            return await GetAsync(runId, accountId, ct);   // بازگشتی برای lazy reset
        }

        // هیچ رکوردی نیست → state پیش‌فرض
        return new LossStreakState(runId, accountId, 0, null, null, null, null);
    }

    public async Task OnPositionClosedAsync(
    Position position, CancellationToken ct = default)
    {
        var opts = _options.CurrentValue.CircuitBreaker;
        if (!opts.Enabled) return;
        if (position.IsPaper && !opts.ApplyToPaper) return;
        if (position.State != PositionState.Closed) return;

        var pnl = position.RealizedPnL.Amount;
        if (pnl == 0m) return;

        var key = (position.RunId, position.TraderAccountId);
        var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await lockObj.WaitAsync(ct);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider
                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

            var (entity, _) = await LoadOrCreateInScopeAsync(
                position.RunId, position.TraderAccountId, uow, ct);

            var wasActive = entity.CircuitBrokenAt.HasValue;

            if (pnl > 0m)
            {
                entity.RecordWin(DateTime.UtcNow);
            }
            else
            {
                entity.RecordLoss(
                    DateTime.UtcNow,
                    opts.ConsecutiveLosses,
                    TimeSpan.FromMinutes(opts.CooldownMinutes));
            }

            await uow.SaveChangesAsync(ct);
            _cache[key] = MapToState(entity);

            if (!wasActive && entity.CircuitBrokenAt.HasValue)
            {
                _eventLogger.Log(
                    stage: TradingStage.Risk,
                    type: TradingEventType.CircuitBreakerTriggered,
                    severity: TradingEventSeverity.Warning,
                    message: $"Circuit Breaker فعال شد: {entity.ConsecutiveLosses} " +
                             $"ضرر متوالی برای (run, account={position.TraderAccountId}). " +
                             $"Cooldown: {opts.CooldownMinutes} دقیقه.",
                    payload: new
                    {
                        consecutiveLosses = entity.ConsecutiveLosses,
                        threshold = opts.ConsecutiveLosses,
                        cooldownMinutes = opts.CooldownMinutes,
                        cooldownExpiresAt = entity.CooldownExpiresAt,
                    },
                    runId: position.RunId,
                    isin: position.Isin,
                    accountId: position.TraderAccountId,
                    positionId: position.Id);
            }
        }
        finally
        {
            lockObj.Release();
        }
    }


    public async Task ResetAsync(
        Guid runId, Guid accountId, string? reason, CancellationToken ct = default)
    {
        var key = (runId, accountId);
        var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await lockObj.WaitAsync(ct);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider
                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

            var existing = await uow.RunAccountLossStreakSpecs.GetBySpecAsync(
                new RunAccountLossStreakByRunAccountSpec(runId, accountId));
            if (existing is null) return;

            var wasActive = existing.CircuitBrokenAt.HasValue;
            existing.Reset();
            await uow.SaveChangesAsync(ct);
            _cache[key] = MapToState(existing);

            if (wasActive)
            {
                _eventLogger.Log(
                    stage: TradingStage.Risk,
                    type: TradingEventType.CircuitBreakerReset,
                    severity: TradingEventSeverity.Info,
                    message: $"Circuit Breaker reset شد. دلیل: {reason ?? "manual"}.",
                    payload: new { reason },
                    runId: runId,
                    accountId: accountId);
            }

            _logger.LogInformation(
                "Reset loss streak for (run={RunId}, account={AccountId}). Reason: {Reason}",
                runId, accountId, reason ?? "manual");
        }
        finally
        {
            lockObj.Release();
        }
    }

    public async Task<int> ResetRunAsync(
        Guid runId, string? reason, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var streaks = (await uow.RunAccountLossStreakSpecs.ListBySpecAsync(
            new RunAccountLossStreaksByRunSpec(runId))).ToList();

        foreach (var s in streaks)
        {
            s.Reset();
            await uow.RunAccountLossStreaks.UpdateAsync(s);
            _cache[(s.RunId, s.AccountId)] = MapToState(s);
        }

        await uow.SaveChangesAsync(ct);
        return streaks.Count;
    }

    public async Task<int> ResetAccountAsync(
        Guid accountId, string? reason, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var streaks = (await uow.RunAccountLossStreakSpecs.ListBySpecAsync(
            new RunAccountLossStreaksByAccountSpec(accountId))).ToList();

        foreach (var s in streaks)
        {
            s.Reset();
            await uow.RunAccountLossStreaks.UpdateAsync(s);
            _cache[(s.RunId, s.AccountId)] = MapToState(s);
        }

        await uow.SaveChangesAsync(ct);
        return streaks.Count;
    }

    public async Task<IReadOnlyList<LossStreakState>> GetActiveCircuitsAsync(
        CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var active = await uow.RunAccountLossStreakSpecs.ListBySpecAsync(
            new ActiveCircuitBreakersSpec());

        return active.Select(MapToState).ToList();
    }

    /* ═══════════════ DB Helpers ═══════════════ */

    private async Task<LossStreakState?> LoadFromDbAsync(
        Guid runId, Guid accountId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var entity = await uow.RunAccountLossStreakSpecs.GetBySpecAsync(
            new RunAccountLossStreakByRunAccountSpec(runId, accountId));

        return entity is null ? null : MapToState(entity);
    }

    private async Task<(RunAccountLossStreak Entity, bool IsNew)> LoadOrCreateInScopeAsync(
    Guid runId, Guid accountId,
    ITraderUnitOfWork<TraderDbContext> uow,
    CancellationToken ct)
    {
        var existing = await uow.RunAccountLossStreakSpecs.GetBySpecAsync(
            new RunAccountLossStreakByRunAccountSpec(runId, accountId));

        if (existing is not null)
            return (existing, false);

        var created = RunAccountLossStreak.Create(runId, accountId);
        await uow.RunAccountLossStreaks.AddAsync(created);
        return (created, true);
    }

    private async Task UpdateEntityAsync(
        RunAccountLossStreak entity, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        await uow.RunAccountLossStreaks.UpdateAsync(entity);
        await uow.SaveChangesAsync(ct);
    }

    private static LossStreakState MapToState(RunAccountLossStreak e) => new(
        RunId: e.RunId,
        AccountId: e.AccountId,
        ConsecutiveLosses: e.ConsecutiveLosses,
        LastLossAt: e.LastLossAt,
        LastWinAt: e.LastWinAt,
        CircuitBrokenAt: e.CircuitBrokenAt,
        CooldownExpiresAt: e.CooldownExpiresAt);
}