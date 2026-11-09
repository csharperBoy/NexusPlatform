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

/// <summary>
/// Singleton. Cache in-memory از LossStreakState per (Run, Account).
///
/// Lifetime note:
///   • ITradingEventLogger Scoped است (per-tick buffer, FlushAsync persists).
///   • این کلاس Singleton است.
///   • برای emit رویدادهای Circuit (که نادرند — یک‌بار در هر activation)،
///     از IServiceScopeFactory یک scope جدید می‌سازیم، Logger Scoped را
///     Resolve می‌کنیم، Log می‌کنیم و همان‌جا FlushAsync می‌کنیم.
///     این الگو دقیقاً همان است که TradingRuntimeWorker.TickAsync در پایان
///     هر tick انجام می‌دهد، فقط با scope مستقل.
/// </summary>
public sealed class RunAccountLossStreakTracker : IRunAccountLossStreakTracker
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TradingRiskOptions> _options;
    private readonly ILogger<RunAccountLossStreakTracker> _logger;

    private readonly ConcurrentDictionary<(Guid RunId, Guid AccountId), LossStreakState> _cache = new();
    private readonly ConcurrentDictionary<(Guid RunId, Guid AccountId), SemaphoreSlim> _locks = new();

    public RunAccountLossStreakTracker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TradingRiskOptions> options,
        ILogger<RunAccountLossStreakTracker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<LossStreakState> GetAsync(
        Guid runId, Guid accountId, CancellationToken ct = default)
    {
        var key = (runId, accountId);

        if (_cache.TryGetValue(key, out var cached))
        {
            // Lazy reset روی Cooldown expiry
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

        var loaded = await LoadFromDbAsync(runId, accountId, ct);
        if (loaded is not null)
        {
            _cache[key] = loaded;
            return await GetAsync(runId, accountId, ct);
        }

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

        var justBroke = false;
        int brokenCount = 0;
        DateTime? brokenExpiresAt = null;

        await lockObj.WaitAsync(ct);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider
                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

            var existing = await uow.RunAccountLossStreakSpecs.GetBySpecAsync(
                new RunAccountLossStreakByRunAccountSpec(
                    position.RunId, position.TraderAccountId));

            RunAccountLossStreak entity;
            if (existing is null)
            {
                entity = RunAccountLossStreak.Create(
                    position.RunId, position.TraderAccountId);
                await uow.RunAccountLossStreaks.AddAsync(entity);
            }
            else
            {
                entity = existing;
            }

            var wasActive = entity.CircuitBrokenAt.HasValue;

            if (pnl > 0m)
            {
                entity.RecordWin(DateTime.UtcNow);
                _logger.LogDebug(
                    "Recorded Win for (run={RunId}, account={AccountId}). Streak reset.",
                    position.RunId, position.TraderAccountId);
            }
            else
            {
                entity.RecordLoss(
                    DateTime.UtcNow,
                    opts.ConsecutiveLosses,
                    TimeSpan.FromMinutes(opts.CooldownMinutes));
                _logger.LogInformation(
                    "Recorded Loss for (run={RunId}, account={AccountId}). " +
                    "Streak={Count}/{Threshold}",
                    position.RunId, position.TraderAccountId,
                    entity.ConsecutiveLosses, opts.ConsecutiveLosses);
            }

            await uow.SaveChangesAsync(ct);
            _cache[key] = MapToState(entity);

            if (!wasActive && entity.CircuitBrokenAt.HasValue)
            {
                justBroke = true;
                brokenCount = entity.ConsecutiveLosses;
                brokenExpiresAt = entity.CooldownExpiresAt;
            }
        }
        finally
        {
            lockObj.Release();
        }

        // Emit OUTSIDE lock (لاگ نباید lock را نگه دارد)
        if (justBroke)
        {
            await LogEventAsync(
                stage: TradingStage.Risk,
                type: TradingEventType.CircuitBreakerTriggered,
                severity: TradingEventSeverity.Warning,
                message: $"Circuit Breaker فعال شد: {brokenCount} " +
                         $"ضرر متوالی برای (run, account={position.TraderAccountId}). " +
                         $"Cooldown: {opts.CooldownMinutes} دقیقه.",
                payload: new
                {
                    consecutiveLosses = brokenCount,
                    threshold = opts.ConsecutiveLosses,
                    cooldownMinutes = opts.CooldownMinutes,
                    cooldownExpiresAt = brokenExpiresAt,
                },
                runId: position.RunId,
                isin: position.Isin,
                accountId: position.TraderAccountId,
                positionId: position.Id,
                ct: ct);
        }
    }

    public async Task ResetAsync(
        Guid runId, Guid accountId, string? reason, CancellationToken ct = default)
    {
        var key = (runId, accountId);
        var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        var wasActive = false;

        await lockObj.WaitAsync(ct);
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider
                .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

            var existing = await uow.RunAccountLossStreakSpecs.GetBySpecAsync(
                new RunAccountLossStreakByRunAccountSpec(runId, accountId));

            if (existing is null) return;

            wasActive = existing.CircuitBrokenAt.HasValue;
            existing.Reset();
            await uow.SaveChangesAsync(ct);
            _cache[key] = MapToState(existing);

            _logger.LogInformation(
                "Reset loss streak for (run={RunId}, account={AccountId}). Reason: {Reason}",
                runId, accountId, reason ?? "manual");
        }
        finally
        {
            lockObj.Release();
        }

        if (wasActive)
        {
            await LogEventAsync(
                stage: TradingStage.Risk,
                type: TradingEventType.CircuitBreakerReset,
                severity: TradingEventSeverity.Info,
                message: $"Circuit Breaker reset شد. دلیل: {reason ?? "manual"}.",
                payload: new { reason },
                runId: runId,
                accountId: accountId,
                ct: ct);
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

    /* ═══════════════ Helpers ═══════════════ */

    /// <summary>
    /// Scope مخصوص لاگ — چون ITradingEventLogger Scoped است و این کلاس Singleton.
    /// Log + Flush در همان Scope → رویداد بلافاصله Persist می‌شود.
    /// فرکانس: فقط هنگام Circuit transition (نادر). هزینه‌ی ساخت Scope قابل قبول.
    /// </summary>
    private async Task LogEventAsync(
        TradingStage stage,
        TradingEventType type,
        TradingEventSeverity severity,
        string message,
        object? payload = null,
        Guid? runId = null,
        string? isin = null,
        Guid? accountId = null,
        Guid? positionId = null,
        CancellationToken ct = default)
    {
        try
        {
            using var logScope = _scopeFactory.CreateScope();
            var logger = logScope.ServiceProvider
                .GetRequiredService<ITradingEventLogger>();

            logger.Log(
                stage: stage,
                type: type,
                severity: severity,
                message: message,
                payload: payload,
                runId: runId,
                isin: isin,
                accountId: accountId,
                positionId: positionId);

            await logger.FlushAsync(ct);
        }
        catch (Exception ex)
        {
            // لاگ رویداد نباید منطق Circuit Breaker را بشکند
            _logger.LogError(ex,
                "Failed to persist {Type} event for (run={RunId}, account={AccountId}).",
                type, runId, accountId);
        }
    }

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

    private static LossStreakState MapToState(RunAccountLossStreak e) => new(
        RunId: e.RunId,
        AccountId: e.AccountId,
        ConsecutiveLosses: e.ConsecutiveLosses,
        LastLossAt: e.LastLossAt,
        LastWinAt: e.LastWinAt,
        CircuitBrokenAt: e.CircuitBrokenAt,
        CooldownExpiresAt: e.CooldownExpiresAt);
}