using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Dashboard;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Dtos.Dashboard;
using Trader.Application.Runtime;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services.Dashboard;

public sealed class DashboardQueryService : IDashboardQueryService
{
    private const int CriticalEventsTake = 10;

    private static readonly TimeZoneInfo TehranTz = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Iran Standard Time" : "Asia/Tehran");

    private readonly TraderDbContext _db;
    private readonly IRiskStateTracker _riskState;
    private readonly IMarketSnapshotStore _snapshotStore;
    private readonly ILogger<DashboardQueryService> _logger;

    public DashboardQueryService(
        TraderDbContext db,
        IRiskStateTracker riskState,
        IMarketSnapshotStore snapshotStore,
        ILogger<DashboardQueryService> logger)
    {
        _db = db;
        _riskState = riskState;
        _snapshotStore = snapshotStore;
        _logger = logger;
    }

    public async Task<DashboardSummaryView> GetSummaryAsync(CancellationToken ct = default)
    {
        var killSwitch = await BuildKillSwitchAsync(ct);
        var runs = await BuildRunsAsync(ct);
        var ordersToday = await BuildOrdersTodayAsync(ct);
        var positions = await BuildPositionsAsync(ct);
        var events = await BuildCriticalEventsAsync(ct);
        var sessions = await BuildAccountSessionsAsync(ct);

        return new DashboardSummaryView(
            KillSwitch: killSwitch,
            Runs: runs,
            OrdersToday: ordersToday,
            Positions: positions,
            RecentCriticalEvents: events,
            AccountsSessions: sessions,
            GeneratedAt: DateTime.UtcNow);
    }

    /* ═══════════════ Kill Switch ═══════════════ */

    private async Task<KillSwitchSummary> BuildKillSwitchAsync(CancellationToken ct)
    {
        await _riskState.RefreshAsync(ct);

        var globalActive = _riskState.IsGlobalKillSwitchActive;
        var activeAccountIds = _riskState.ActiveAccountKillSwitches.ToList();

        var activeRecords = await _db.RiskKillSwitches.AsNoTracking()
            .Where(k => k.DeactivatedAt == null)
            .ToListAsync(ct);

        var globalRecord = activeRecords.FirstOrDefault(r => r.AccountId == null);

        var accountRecords = activeRecords
            .Where(r => r.AccountId != null)
            .ToList();

        var accountIds = accountRecords.Select(r => r.AccountId!.Value).Distinct().ToList();
        var accountNames = accountIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.Accounts.AsNoTracking()
                .Where(a => accountIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.Name, ct);

        var accountViews = accountRecords
            .Select(r => new KillSwitchAccountView(
                AccountId: r.AccountId!.Value,
                AccountName: accountNames.GetValueOrDefault(r.AccountId.Value, "—"),
                Reason: r.Reason,
                ActivatedAt: r.ActivatedAt))
            .ToList();

        return new KillSwitchSummary(
            GlobalActive: globalActive,
            GlobalReason: globalRecord?.Reason,
            GlobalActivatedAt: globalRecord?.ActivatedAt,
            ActiveAccounts: accountViews);
    }

    /* ═══════════════ Runs ═══════════════ */

    private async Task<RunsSummary> BuildRunsAsync(CancellationToken ct)
    {
        var grouped = await _db.TradingRuns.AsNoTracking()
            .GroupBy(r => r.State)
            .Select(g => new { State = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        int Get(TradingRunState s) => grouped.FirstOrDefault(g => g.State == s)?.Count ?? 0;

        return new RunsSummary(
            Running: Get(TradingRunState.Running),
            Ready: Get(TradingRunState.Ready),
            Stopped: Get(TradingRunState.Stopped),
            Faulted: Get(TradingRunState.Faulted),
            Created: Get(TradingRunState.Created));
    }

    /* ═══════════════ Orders Today ═══════════════ */

    private async Task<OrdersTodaySummary> BuildOrdersTodayAsync(CancellationToken ct)
    {
        var todayTehran = TehranDate.Today();
        var todayStartTehran = todayTehran.ToDateTime(TimeOnly.MinValue);
        var unspecified = DateTime.SpecifyKind(todayStartTehran, DateTimeKind.Unspecified);
        var todayStartUtc = TimeZoneInfo.ConvertTimeToUtc(unspecified, TehranTz);

        var grouped = await _db.BrokerOrders.AsNoTracking()
            .Where(o => o.SubmittedAt >= todayStartUtc)
            .GroupBy(o => new { o.IsPaper, o.State })
            .Select(g => new
            {
                g.Key.IsPaper,
                g.Key.State,
                Count = g.Count(),
            })
            .ToListAsync(ct);

        var total = grouped.Sum(g => g.Count);
        var paper = grouped.Where(g => g.IsPaper).Sum(g => g.Count);
        var live = grouped.Where(g => !g.IsPaper).Sum(g => g.Count);

        int CountState(OrderStateKind s) =>
            grouped.Where(g => g.State == s).Sum(g => g.Count);

        return new OrdersTodaySummary(
            TodayStartUtc: todayStartUtc,
            Total: total,
            Paper: paper,
            Live: live,
            Pending: CountState(OrderStateKind.Pending),
            Open: CountState(OrderStateKind.Open),
            PartiallyExecuted: CountState(OrderStateKind.PartiallyExecuted),
            FullyExecuted: CountState(OrderStateKind.FullyExecuted),
            Canceled: CountState(OrderStateKind.Canceled),
            Rejected: CountState(OrderStateKind.Rejected),
            Expired: CountState(OrderStateKind.Expired));
    }

    /* ═══════════════ Positions ═══════════════ */

    private async Task<PositionsSummary> BuildPositionsAsync(CancellationToken ct)
    {
        var positions = await _db.Positions.AsNoTracking()
            .Where(p => p.State != PositionState.Closed)
            .ToListAsync(ct);

        if (positions.Count == 0)
        {
            return new PositionsSummary(0, 0, 0, 0m, Array.Empty<PositionBriefView>());
        }

        var accountIds = positions.Select(p => p.TraderAccountId).Distinct().ToList();
        var accountNames = await _db.Accounts.AsNoTracking()
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Name, ct);

        var isins = positions.Select(p => p.Isin).Distinct().ToList();
        var symbolNames = await _db.Instruments.AsNoTracking()
            .Where(i => isins.Contains(i.Isin))
            .ToDictionaryAsync(i => i.Isin, i => i.SymbolName, ct);

        var views = new List<PositionBriefView>(positions.Count);
        decimal totalUnrealized = 0m;
        var paperCount = 0;
        var liveCount = 0;

        foreach (var pos in positions)
        {
            if (pos.IsPaper) paperCount++; else liveCount++;

            var snapshot = _snapshotStore.GetLatest(pos.Isin);
            decimal? currentPrice = snapshot is { LastTradedPrice: > 0 }
                ? snapshot.LastTradedPrice
                : null;

            decimal? unrealized = currentPrice.HasValue
                ? (currentPrice.Value - pos.AverageEntryPrice.Value) * pos.Quantity.Value
                : null;

            if (unrealized.HasValue) totalUnrealized += unrealized.Value;

            views.Add(new PositionBriefView(
                Id: pos.Id,
                RunId: pos.RunId,
                AccountId: pos.TraderAccountId,
                AccountName: accountNames.GetValueOrDefault(pos.TraderAccountId, "—"),
                Isin: pos.Isin,
                SymbolName: symbolNames.GetValueOrDefault(pos.Isin, "—"),
                Quantity: pos.Quantity.Value,
                AverageEntryPrice: pos.AverageEntryPrice.Value,
                CurrentPrice: currentPrice,
                UnrealizedPnL: unrealized,
                IsPaper: pos.IsPaper));
        }

        return new PositionsSummary(
            TotalCount: positions.Count,
            PaperCount: paperCount,
            LiveCount: liveCount,
            TotalUnrealizedPnL: totalUnrealized,
            Items: views.OrderBy(v => v.SymbolName).ToList());
    }

    /* ═══════════════ Recent Critical Events ═══════════════ */

    private async Task<IReadOnlyList<CriticalEventView>> BuildCriticalEventsAsync(
        CancellationToken ct)
    {
        var minSeverity = TradingEventSeverity.Warning;

        var events = await _db.TradingEvents.AsNoTracking()
            .Where(e => (int)e.Severity >= (int)minSeverity)
            .OrderByDescending(e => e.OccurredAt)
            .Take(CriticalEventsTake)
            .ToListAsync(ct);

        return events.Select(e => new CriticalEventView(
            Id: e.Id,
            OccurredAt: e.OccurredAt,
            Severity: (int)e.Severity,
            SeverityName: e.Severity.ToString(),
            Type: (int)e.Type,
            TypeName: e.Type.ToString(),
            Isin: e.Isin,
            RunId: e.RunId,
            Message: e.Message))
            .ToList();
    }

    /* ═══════════════ Accounts Session ═══════════════ */

    private async Task<AccountsSessionSummary> BuildAccountSessionsAsync(
        CancellationToken ct)
    {
        var accounts = await _db.Accounts.AsNoTracking().ToListAsync(ct);

        var total = accounts.Count;
        var valid = 0;
        var expired = 0;
        var empty = 0;
        var invalid = 0;

        foreach (var a in accounts)
        {
            switch (a.GetSessionStatus())
            {
                case SessionStatus.Valid: valid++; break;
                case SessionStatus.Expired: expired++; break;
                case SessionStatus.Empty: empty++; break;
                case SessionStatus.Invalid: invalid++; break;
            }
        }

        return new AccountsSessionSummary(total, valid, expired, empty, invalid);
    }
}