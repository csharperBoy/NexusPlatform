using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Brokers;
using Trader.Application.Abstractions.Brokers.Queries;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Reconciliation;
using Trader.Application.Dtos.Account;
using Trader.Application.Dtos.Reconciliation;
using Trader.Application.Specifications.TradingRuns;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.ExternalApiClient;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Reconciliation;

public sealed class ReconciliationService : IReconciliationService
{
    private readonly ITraderUnitOfWork<TraderDbContext> _uow;
    private readonly IBrokerSessionProvider _sessionProvider;
    private readonly IBrokerClientFactory _brokerFactory;
    private readonly ITradingEventLogger _eventLogger;
    private readonly ILogger<ReconciliationService> _logger;

    public ReconciliationService(
        ITraderUnitOfWork<TraderDbContext> uow,
        IBrokerSessionProvider sessionProvider,
        IBrokerClientFactory brokerFactory,
        ITradingEventLogger eventLogger,
        ILogger<ReconciliationService> logger)
    {
        _uow = uow;
        _sessionProvider = sessionProvider;
        _brokerFactory = brokerFactory;
        _eventLogger = eventLogger;
        _logger = logger;
    }

    public async Task<ReconciliationResult> ReconcileAllAsync(CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;

        _eventLogger.Log(
            stage: TradingStage.Reconciliation,
            type: TradingEventType.ReconciliationStarted,
            severity: TradingEventSeverity.Info,
            message: "فرآیند تطبیق Positionها با کارگزاری شروع شد.");

        try
        {
            // ─── ۱. Accounts برای Reconcile ───
            var accountIds = await CollectAccountsToReconcileAsync(ct);

            if (accountIds.Count == 0)
            {
                var empty = BuildResult(
                    new List<AccountReconciliation>(), startedAt);

                _eventLogger.Log(
                    stage: TradingStage.Reconciliation,
                    type: TradingEventType.ReconciliationCompleted,
                    severity: TradingEventSeverity.Info,
                    message: "تطبیق انجام شد: هیچ Accountای برای بررسی وجود ندارد.");

                return empty;
            }

            _logger.LogInformation(
                "Reconciliation starting for {Count} accounts.", accountIds.Count);

            // ─── ۲. Reconcile هر Account ───
            var details = new List<AccountReconciliation>(accountIds.Count);

            foreach (var accountId in accountIds)
            {
                if (ct.IsCancellationRequested) break;

                AccountReconciliation detail;
                try
                {
                    detail = await ReconcileAccountAsync(accountId, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Reconciliation failed for account {AccountId}.", accountId);

                    detail = new AccountReconciliation(
                        accountId, string.Empty, default,
                        Success: false,
                        ErrorMessage: ex.Message,
                        DbPositionCount: 0, BrokerPositionCount: 0,
                        MatchedCount: 0,
                        Mismatches: Array.Empty<PositionMismatch>());
                }

                details.Add(detail);
            }

            var result = BuildResult(details, startedAt);

            // ─── ۳. Log نتیجه ───
            var totalMismatches =
                result.QuantityMismatches
                + result.DbOnlyPositions
                + result.BrokerOnlyPositions;

            if (totalMismatches == 0 && result.FailedAccounts == 0)
            {
                _eventLogger.Log(
                    stage: TradingStage.Reconciliation,
                    type: TradingEventType.ReconciliationCompleted,
                    severity: TradingEventSeverity.Info,
                    message: $"تطبیق موفق: {result.AccountsChecked} حساب، " +
                             $"{result.MatchedPositions} موقعیت مطابق. بدون مغایرت.",
                    payload: new
                    {
                        accountsChecked = result.AccountsChecked,
                        matched = result.MatchedPositions,
                        durationMs = result.DurationMs,
                    });
            }
            else
            {
                _eventLogger.Log(
                    stage: TradingStage.Reconciliation,
                    type: TradingEventType.ReconciliationCompleted,
                    severity: TradingEventSeverity.Warning,
                    message: $"تطبیق انجام شد: {totalMismatches} مغایرت در " +
                             $"{result.AccountsChecked} حساب (مطابق: {result.MatchedPositions}, " +
                             $"اختلاف تعداد: {result.QuantityMismatches}, " +
                             $"فقط DB: {result.DbOnlyPositions}, " +
                             $"فقط Broker: {result.BrokerOnlyPositions}, " +
                             $"حساب‌های خطادار: {result.FailedAccounts}).",
                    payload: new
                    {
                        accountsChecked = result.AccountsChecked,
                        matched = result.MatchedPositions,
                        quantityMismatches = result.QuantityMismatches,
                        dbOnly = result.DbOnlyPositions,
                        brokerOnly = result.BrokerOnlyPositions,
                        failedAccounts = result.FailedAccounts,
                        durationMs = result.DurationMs,
                    });
            }

            _logger.LogInformation(
                "Reconciliation completed. Accounts={Accounts} Matched={Matched} " +
                "QuantityMismatch={QM} DbOnly={DbO} BrokerOnly={BrO} Failed={Failed} Duration={Dur}ms",
                result.AccountsChecked, result.MatchedPositions,
                result.QuantityMismatches, result.DbOnlyPositions,
                result.BrokerOnlyPositions, result.FailedAccounts, result.DurationMs);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reconciliation failed (global).");

            _eventLogger.Log(
                stage: TradingStage.Reconciliation,
                type: TradingEventType.ReconciliationFailed,
                severity: TradingEventSeverity.Error,
                message: $"تطبیق ناموفق بود: {ex.Message}",
                payload: new
                {
                    errorType = ex.GetType().Name,
                    errorMessage = ex.Message,
                });

            throw;
        }
    }

    public async Task<AccountReconciliation> ReconcileAccountAsync(
        Guid accountId, CancellationToken ct = default)
    {
        // ─── ۱. Account ───
        var account = await _uow.Accounts.GetByIdAsync(accountId);
        if (account is null)
        {
            return new AccountReconciliation(
                accountId, string.Empty, default,
                Success: false,
                ErrorMessage: "Account not found.",
                DbPositionCount: 0, BrokerPositionCount: 0,
                MatchedCount: 0,
                Mismatches: Array.Empty<PositionMismatch>());
        }

        // ─── ۲. DB Positions (non-paper, open) ───
        var dbPositions = (await _uow.Positions.GetAllAsync())
            .Where(p => p.TraderAccountId == accountId
                     && !p.IsPaper
                     && p.State != PositionState.Closed
                     && p.Quantity.Value > 0)
            .ToList();

        // ─── ۳. Session ───
        ExternalSystemSession session;
        try
        {
            session = await _sessionProvider.GetSessionAsync(accountId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Session unavailable for account {AccountId}. Skipping reconciliation.",
                accountId);

            _eventLogger.Log(
                stage: TradingStage.Reconciliation,
                type: TradingEventType.ReconciliationFailed,
                severity: TradingEventSeverity.Warning,
                message: $"تطبیق حساب «{account.Name}» انجام نشد: " +
                         $"Session موجود نیست ({ex.Message}).",
                payload: new
                {
                    accountId,
                    errorType = ex.GetType().Name,
                },
                accountId: accountId);

            return new AccountReconciliation(
                accountId, account.Name, account.Broker,
                Success: false,
                ErrorMessage: $"Session unavailable: {ex.Message}",
                DbPositionCount: dbPositions.Count,
                BrokerPositionCount: 0,
                MatchedCount: 0,
                Mismatches: Array.Empty<PositionMismatch>());
        }

        // ─── ۴. Broker Portfolio ───
        List<PortfolioPerformanceDto> brokerPortfolio;
        try
        {
            var reader = _brokerFactory.Get<IBrokerAccountReader>(account.Broker);
            brokerPortfolio = await reader.GetPortfolioPerformanceAsync(session, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to fetch portfolio for account {AccountId}.", accountId);

            _eventLogger.Log(
                stage: TradingStage.Reconciliation,
                type: TradingEventType.ReconciliationFailed,
                severity: TradingEventSeverity.Warning,
                message: $"دریافت Portfolio از Broker برای «{account.Name}» ناموفق بود: {ex.Message}",
                payload: new
                {
                    accountId,
                    errorType = ex.GetType().Name,
                    errorMessage = ex.Message,
                },
                accountId: accountId);

            return new AccountReconciliation(
                accountId, account.Name, account.Broker,
                Success: false,
                ErrorMessage: $"Portfolio fetch failed: {ex.Message}",
                DbPositionCount: dbPositions.Count,
                BrokerPositionCount: 0,
                MatchedCount: 0,
                Mismatches: Array.Empty<PositionMismatch>());
        }

        // ─── ۵. Aggregate per ISIN ───
        var dbByIsin = dbPositions
            .GroupBy(p => p.Isin)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Qty = g.Sum(p => p.Quantity.Value),
                    AvgPrice = g.Sum(p => p.AverageEntryPrice.Value * p.Quantity.Value)
                               / Math.Max(1, g.Sum(p => p.Quantity.Value)),
                });

        var brokerByIsin = brokerPortfolio
            .Where(p => !string.IsNullOrWhiteSpace(GetIsin(p)) && GetQuantity(p) > 0)
            .GroupBy(p => GetIsin(p)!)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    Qty = g.Sum(p => GetQuantity(p)),
                    AvgPrice = 0m,   // اگر Broker میانگین می‌دهد، اضافه کن
                });

        // ─── ۶. Match ───
        var allIsins = dbByIsin.Keys.Union(brokerByIsin.Keys).ToList();
        var mismatches = new List<PositionMismatch>();
        var matched = 0;

        foreach (var isin in allIsins)
        {
            var inDb = dbByIsin.TryGetValue(isin, out var dbInfo);
            var inBroker = brokerByIsin.TryGetValue(isin, out var brokerInfo);

            if (inDb && inBroker && dbInfo!.Qty == brokerInfo!.Qty)
            {
                matched++;
                continue;
            }

            PositionMismatchKind kind;
            long dbQty = inDb ? dbInfo!.Qty : 0;
            long brokerQty = inBroker ? brokerInfo!.Qty : 0;
            decimal dbAvg = inDb ? dbInfo!.AvgPrice : 0m;
            decimal brokerAvg = inBroker ? brokerInfo!.AvgPrice : 0m;

            if (inDb && inBroker)
                kind = PositionMismatchKind.QuantityMismatch;
            else if (inDb)
                kind = PositionMismatchKind.DbOnly;
            else
                kind = PositionMismatchKind.BrokerOnly;

            var mismatch = new PositionMismatch(
                isin, kind, dbQty, brokerQty, dbAvg, brokerAvg);

            mismatches.Add(mismatch);

            // ─── Log per-mismatch ───
            var (severity, message) = kind switch
            {
                PositionMismatchKind.QuantityMismatch =>
                    (TradingEventSeverity.Error,
                     $"مغایرت تعداد در «{isin}» حساب «{account.Name}»: " +
                     $"DB={dbQty:N0}, Broker={brokerQty:N0}"),
                PositionMismatchKind.DbOnly =>
                    (TradingEventSeverity.Warning,
                     $"Position «{isin}» فقط در DB است ({dbQty:N0} سهم) — " +
                     $"Broker ندارد. حساب «{account.Name}»."),
                PositionMismatchKind.BrokerOnly =>
                    (TradingEventSeverity.Warning,
                     $"Position «{isin}» فقط در Broker است ({brokerQty:N0} سهم) — " +
                     $"DB ندارد. حساب «{account.Name}»."),
                _ => (TradingEventSeverity.Info, "—"),
            };

            _eventLogger.Log(
                stage: TradingStage.Reconciliation,
                type: TradingEventType.ReconciliationMismatchFound,
                severity: severity,
                message: message,
                payload: new
                {
                    kind = kind.ToString(),
                    dbQuantity = dbQty,
                    brokerQuantity = brokerQty,
                    dbAvgPrice = dbAvg,
                    brokerAvgPrice = brokerAvg,
                },
                isin: isin,
                accountId: accountId);
        }

        return new AccountReconciliation(
            accountId,
            account.Name,
            account.Broker,
            Success: true,
            ErrorMessage: null,
            DbPositionCount: dbPositions.Count,
            BrokerPositionCount: brokerPortfolio.Count,
            MatchedCount: matched,
            Mismatches: mismatches);
    }

    /* ═══════════════ Helpers ═══════════════ */

    private async Task<List<Guid>> CollectAccountsToReconcileAsync(CancellationToken ct)
    {
        var accountIds = new HashSet<Guid>();

        // ۱. Accounts با Position باز non-paper
        var openPositions = (await _uow.Positions.GetAllAsync())
            .Where(p => !p.IsPaper
                     && p.State != PositionState.Closed
                     && p.Quantity.Value > 0);

        foreach (var p in openPositions)
            accountIds.Add(p.TraderAccountId);

        // ۲. Accounts از Runهای Runnable (برای کشف Broker-only Positionها)
        var runnableSpec = new RunnableTradingRunsSpec();
        var runnableRuns = await _uow.TradingRunSpecs.ListBySpecAsync(runnableSpec);

        foreach (var run in runnableRuns)
            foreach (var acc in run.Accounts)
                accountIds.Add(acc.TraderAccountId);

        return accountIds.ToList();
    }

    private static ReconciliationResult BuildResult(
        List<AccountReconciliation> details, DateTime startedAt)
    {
        var completedAt = DateTime.UtcNow;

        return new ReconciliationResult(
            AccountsChecked: details.Count,
            MatchedPositions: details.Sum(d => d.MatchedCount),
            QuantityMismatches: details.Sum(d =>
                d.Mismatches.Count(m => m.Kind == PositionMismatchKind.QuantityMismatch)),
            DbOnlyPositions: details.Sum(d =>
                d.Mismatches.Count(m => m.Kind == PositionMismatchKind.DbOnly)),
            BrokerOnlyPositions: details.Sum(d =>
                d.Mismatches.Count(m => m.Kind == PositionMismatchKind.BrokerOnly)),
            FailedAccounts: details.Count(d => !d.Success),
            AccountDetails: details,
            StartedAt: startedAt,
            CompletedAt: completedAt,
            DurationMs: (long)(completedAt - startedAt).TotalMilliseconds);
    }

    // PortfolioPerformanceDto — اگر فیلدها متفاوت است، این دو متد را تنظیم کن
    private static string? GetIsin(PortfolioPerformanceDto p)
    {
        // DTO فعلی — فرض: Isin یا SymbolIsin
        // اگر نام متفاوت است، اینجا اصلاح کن
        var prop = typeof(PortfolioPerformanceDto).GetProperty("SymbolIsin")
                   ?? typeof(PortfolioPerformanceDto).GetProperty("Isin");
        return prop?.GetValue(p) as string;
    }

    private static long GetQuantity(PortfolioPerformanceDto p)
    {
        var prop = typeof(PortfolioPerformanceDto).GetProperty("Quantity");
        var val = prop?.GetValue(p);
        return val switch
        {
            long l => l,
            int i => i,
            _ => 0L,
        };
    }
}