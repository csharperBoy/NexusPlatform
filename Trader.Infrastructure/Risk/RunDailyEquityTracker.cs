using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Positions;
using Trader.Application.Specifications.Risk;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Risk;

/// <summary>
/// Singleton. Cache in-memory از Opening Equity + Last Known Cash per Run.
/// Persistence برای Opening در DB (survive restart).
///
/// Thread-safety:
///   • _openings، _lastCash: ConcurrentDictionary
///   • _locks: per-Run SemaphoreSlim (serialize capture + refresh)
///
/// Design:
///   • Lazy capture: اولین intent روز trigger می‌کند
///   • Refresh از Broker هر 15 دقیقه (configurable)
///   • Position Value هر بار live از DB + Snapshot Store
///   • Skip کردن Paper Mode (نه capture)
/// </summary>
public sealed class RunDailyEquityTracker : IRunDailyEquityTracker
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<TradingRiskOptions> _options;
    private readonly ILogger<RunDailyEquityTracker> _logger;

    private readonly ConcurrentDictionary<(Guid RunId, DateOnly Date), RunDailyEquityOpening> _openings = new();
    private readonly ConcurrentDictionary<Guid, BrokerCashState> _lastCash = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _locks = new();

    public RunDailyEquityTracker(
        IRunExecutionContextCache runCache,
        IMarketSnapshotStore snapshotStore,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<TradingRiskOptions> options,
        ILogger<RunDailyEquityTracker> logger)
    {
        _runCache = runCache;
        _snapshotStore = snapshotStore;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<RunDailyEquityOpening> GetOrCaptureOpeningAsync(
        Guid runId, CancellationToken ct = default)
    {
        var today = TehranDate.Today();
        var key = (runId, today);

        if (_openings.TryGetValue(key, out var cached))
            return cached;

        var lockObj = GetOrCreateLock(runId);
        await lockObj.WaitAsync(ct);
        try
        {
            // Double-check بعد از lock
            if (_openings.TryGetValue(key, out cached))
                return cached;

            // از DB (اگر روز قبل capture شده)
            var fromDb = await LoadFromDbAsync(runId, today, ct);
            if (fromDb is not null)
            {
                _openings[key] = fromDb;
                _lastCash[runId] = new BrokerCashState(
                    fromDb.OpeningCash, fromDb.CapturedAt);
                return fromDb;
            }

            // Capture جدید
            var cash = await FetchCashFromBrokerAsync(runId, ct);
            var positionValue = await ComputePositionValueAsync(runId, ct);

            var snapshot = RunDailyEquitySnapshot.Create(
                runId, today, cash, positionValue);
            await SaveToDbAsync(snapshot, ct);

            var opening = MapToOpening(snapshot);
            _openings[key] = opening;
            _lastCash[runId] = new BrokerCashState(cash, DateTime.UtcNow);

            _logger.LogInformation(
                "Captured Daily Opening for run {RunId} on {Date}: " +
                "cash={Cash:N0}, positionValue={PosVal:N0}, equity={Equity:N0}",
                runId, today, cash, positionValue, opening.OpeningEquity);

            return opening;
        }
        finally
        {
            lockObj.Release();
        }
    }

    public async Task<RunDailyEquityCurrent> GetCurrentAsync(
        Guid runId, CancellationToken ct = default)
    {
        var opening = await GetOrCaptureOpeningAsync(runId, ct);

        // ── Refresh Cash اگر لازم ──
        var interval = _options.CurrentValue.DailyLossLimit.BrokerRefreshInterval;
        var now = DateTime.UtcNow;
        var last = _lastCash.GetOrAdd(
            runId, _ => new BrokerCashState(opening.OpeningCash, opening.CapturedAt));

        if (now - last.RefreshedAt > interval)
        {
            var lockObj = GetOrCreateLock(runId);
            // Non-blocking: اگر intent دیگری در حال refresh است، با stale ادامه می‌دهیم.
            // Safe: stale cash ممکن است loss را بیش از واقع نشان دهد → Block زودتر (امن)
            if (await lockObj.WaitAsync(0, ct))
            {
                try
                {
                    // Double-check
                    last = _lastCash.GetOrAdd(runId, _ => last);
                    if (DateTime.UtcNow - last.RefreshedAt > interval)
                    {
                        try
                        {
                            var freshCash = await FetchCashFromBrokerAsync(runId, ct);
                            last = new BrokerCashState(freshCash, DateTime.UtcNow);
                            _lastCash[runId] = last;

                            _logger.LogDebug(
                                "Refreshed cash for run {RunId}: {Cash:N0}",
                                runId, freshCash);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Failed to refresh cash for run {RunId}. " +
                                "Using stale cash {Stale:N0} (from {Age:F0}s ago).",
                                runId, last.Cash,
                                (DateTime.UtcNow - last.RefreshedAt).TotalSeconds);
                            // Keep stale — do not throw
                        }
                    }
                }
                finally
                {
                    lockObj.Release();
                }
            }
        }

        var currentPositionValue = await ComputePositionValueAsync(runId, ct);

        return new RunDailyEquityCurrent(
            Opening: opening,
            CurrentCash: last.Cash,
            CurrentPositionValue: currentPositionValue,
            LastCashRefreshAt: last.RefreshedAt);
    }

    /* ═══════════════ DB ═══════════════ */

    private async Task<RunDailyEquityOpening?> LoadFromDbAsync(
        Guid runId, DateOnly date, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var entity = await uow.RunDailyEquitySnapshotSpecs.GetBySpecAsync(
            new RunDailyEquitySnapshotByRunAndDateSpec(runId, date));

        return entity is null ? null : MapToOpening(entity);
    }

    private async Task SaveToDbAsync(
        RunDailyEquitySnapshot snapshot, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        await uow.RunDailyEquitySnapshots.AddAsync(snapshot);
        await uow.SaveChangesAsync(ct);
    }

    /* ═══════════════ Broker ═══════════════ */

    private async Task<decimal> FetchCashFromBrokerAsync(
        Guid runId, CancellationToken ct)
    {
        var ctx = _runCache.Get(runId)
            ?? throw new InvalidOperationException(
                $"Run {runId} not in cache — cannot fetch cash.");

        if (ctx.AccountIds.Count == 0)
            return 0m;

        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();
        var sessionProvider = scope.ServiceProvider
            .GetRequiredService<IBrokerSessionProvider>();
        var brokerFactory = scope.ServiceProvider
            .GetRequiredService<IBrokerClientFactory>();

        decimal totalCash = 0m;
        foreach (var accountId in ctx.AccountIds)
        {
            var account = await uow.Accounts.GetByIdAsync(accountId);
            if (account is null)
            {
                _logger.LogWarning(
                    "Account {AccountId} in run {RunId} not found; skipping.",
                    accountId, runId);
                continue;
            }

            var session = await sessionProvider.GetSessionAsync(accountId, ct);
            var reader = brokerFactory.Get<IBrokerAccountReader>(account.Broker);
            var money = await reader.GetCashBalanceAsync(session, ct);
            totalCash += money.BuyPowerT0;
        }

        return totalCash;
    }

    /* ═══════════════ Position Value ═══════════════ */

    private async Task<decimal> ComputePositionValueAsync(
        Guid runId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var positions = (await uow.PositionSpecs.ListBySpecAsync(
            new OpenPositionsByRunSpec(runId))).ToList();

        decimal total = 0m;
        foreach (var pos in positions)
        {
            // Paper positions در equity روزانه Live نباید حساب شوند
            if (pos.IsPaper) continue;

            var snapshot = _snapshotStore.GetLatest(pos.Isin);
            var price = snapshot is { LastTradedPrice: > 0 }
                ? snapshot.LastTradedPrice
                : pos.AverageEntryPrice.Value;   // fallback: avg price

            total += pos.Quantity.Value * price;
        }

        return total;
    }

    /* ═══════════════ Helpers ═══════════════ */

    private SemaphoreSlim GetOrCreateLock(Guid runId)
        => _locks.GetOrAdd(runId, _ => new SemaphoreSlim(1, 1));

    private static RunDailyEquityOpening MapToOpening(RunDailyEquitySnapshot e)
        => new(
            RunId: e.RunId,
            TradingDate: e.TradingDate,
            OpeningCash: e.OpeningCash,
            OpeningPositionValue: e.OpeningPositionValue,
            OpeningEquity: e.OpeningEquity,
            CapturedAt: e.CapturedAt);

    private sealed record BrokerCashState(decimal Cash, DateTime RefreshedAt);
}