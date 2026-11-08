using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Application.Specifications.Positions;
using Trader.Application.Specifications.Risk;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Risk;

public sealed class RunAccountDailyEquityTracker : IRunDailyEquityTracker
{
    private readonly IRunExecutionContextCache _runCache;
    private readonly IAccountCashCache _cashCache;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RunAccountDailyEquityTracker> _logger;

    private readonly ConcurrentDictionary<(Guid RunId, Guid AccountId, DateOnly Date), RunAccountDailyEquityOpening> _openings = new();
    private readonly ConcurrentDictionary<(Guid RunId, Guid AccountId), SemaphoreSlim> _locks = new();

    public RunAccountDailyEquityTracker(
        IRunExecutionContextCache runCache,
        IAccountCashCache cashCache,
        IMarketSnapshotStore snapshotStore,
        IServiceScopeFactory scopeFactory,
        ILogger<RunAccountDailyEquityTracker> logger)
    {
        _runCache = runCache;
        _cashCache = cashCache;
        _snapshotStore = snapshotStore;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<RunAccountDailyEquityOpening> GetOrCaptureOpeningAsync(
        Guid runId, Guid accountId, CancellationToken ct = default)
    {
        var today = TehranDate.Today();
        var key = (runId, accountId, today);

        if (_openings.TryGetValue(key, out var cached))
            return cached;

        var lockObj = GetLock(runId, accountId);
        await lockObj.WaitAsync(ct);
        try
        {
            if (_openings.TryGetValue(key, out cached))
                return cached;

            var fromDb = await LoadFromDbAsync(runId, accountId, today, ct);
            if (fromDb is not null)
            {
                _openings[key] = fromDb;
                return fromDb;
            }

            var cash = await _cashCache.GetOrRefreshAsync(accountId, ct);
            var positionValue = await ComputePositionValueAsync(runId, accountId, ct);

            var snapshot = RunAccountDailyEquitySnapshot.Create(
                runId, accountId, today, cash.BuyPowerT0, positionValue);

            await SaveToDbAsync(snapshot, ct);

            var opening = MapToOpening(snapshot);
            _openings[key] = opening;

            _logger.LogInformation(
                "Captured Daily Opening for (run={RunId}, account={AccountId}) on {Date}: " +
                "cash={Cash:N0}, positionValue={PosVal:N0}, equity={Equity:N0}",
                runId, accountId, today, cash.BuyPowerT0, positionValue, opening.OpeningEquity);

            return opening;
        }
        finally
        {
            lockObj.Release();
        }
    }

    public async Task<RunAccountDailyEquityCurrent> GetCurrentAsync(
        Guid runId, Guid accountId, CancellationToken ct = default)
    {
        var opening = await GetOrCaptureOpeningAsync(runId, accountId, ct);

        // ── Cash از Cache (بعد از Invalidate، GetOrRefreshAsync خودش refresh می‌کند) ──
        var cash = await _cashCache.GetOrRefreshAsync(accountId, ct);

        var positionValue = await ComputePositionValueAsync(runId, accountId, ct);

        return new RunAccountDailyEquityCurrent(
            Opening: opening,
            CurrentCash: cash.BuyPowerT0,
            CurrentPositionValue: positionValue,
            LastCashRefreshAt: cash.RefreshedAt);
    }

    /* ═══════════════ DB ═══════════════ */

    private async Task<RunAccountDailyEquityOpening?> LoadFromDbAsync(
        Guid runId, Guid accountId, DateOnly date, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var entity = await uow.RunAccountDailyEquitySnapshotSpecs.GetBySpecAsync(
            new RunAccountDailyEquitySnapshotByRunAccountDateSpec(runId, accountId, date));

        return entity is null ? null : MapToOpening(entity);
    }

    private async Task SaveToDbAsync(
        RunAccountDailyEquitySnapshot snapshot, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        await uow.RunAccountDailyEquitySnapshots.AddAsync(snapshot);
        await uow.SaveChangesAsync(ct);
    }

    /* ═══════════════ Position Value ═══════════════ */

    private async Task<decimal> ComputePositionValueAsync(
        Guid runId, Guid accountId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider
            .GetRequiredService<ITraderUnitOfWork<TraderDbContext>>();

        var positions = (await uow.PositionSpecs.ListBySpecAsync(
            new OpenPositionsByRunSpec(runId)))
            .Where(p => p.TraderAccountId == accountId && !p.IsPaper)
            .ToList();

        decimal total = 0m;
        foreach (var pos in positions)
        {
            var snapshot = _snapshotStore.GetLatest(pos.Isin);
            var price = snapshot is { LastTradedPrice: > 0 }
                ? snapshot.LastTradedPrice
                : pos.AverageEntryPrice.Value;
            total += pos.Quantity.Value * price;
        }

        return total;
    }

    /* ═══════════════ Helpers ═══════════════ */

    private SemaphoreSlim GetLock(Guid runId, Guid accountId)
        => _locks.GetOrAdd((runId, accountId), _ => new SemaphoreSlim(1, 1));

    private static RunAccountDailyEquityOpening MapToOpening(RunAccountDailyEquitySnapshot e)
        => new(
            RunId: e.RunId,
            AccountId: e.AccountId,
            TradingDate: e.TradingDate,
            OpeningCash: e.OpeningCash,
            OpeningPositionValue: e.OpeningPositionValue,
            OpeningEquity: e.OpeningEquity,
            CapturedAt: e.CapturedAt);
}