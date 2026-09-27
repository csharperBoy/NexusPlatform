using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Scheduler.Infrastructure.Jobs;
using Trader.Application.Abstractions;
using Trader.Application.Brokers;
using Trader.Application.Dtos;
using Trader.Application.Scheduler.Services;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Brokers.EasyTrader.Internal;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Scheduler
{
    public class PlanExecutor : IPlanExecutor
    {
        private readonly IRepository<TraderDbContext, SchedulePlan, Guid> _planRepository;
        private readonly IRepository<TraderDbContext, TraderAccount, Guid> _accountRepository;
        private readonly IRepository<TraderDbContext, TraderSymbol, Guid> _symbolRepository;
        private readonly IRepository<TraderDbContext, ExecutionLog, Guid> _logRepository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        private readonly IBrokerClientFactory _brokerFactory;
        private readonly ISecretProtector _protector;
        private readonly IServerClockQueryService _clock;
        private readonly ILogger<PlanExecutor> _logger;

        private static readonly TimeSpan IranOffset = TimeSpan.FromHours(3.5);
        private const long MAX_LATE_MS = 5000;

        public PlanExecutor(
            IRepository<TraderDbContext, SchedulePlan, Guid> planRepository,
            IRepository<TraderDbContext, TraderAccount, Guid> accountRepository,
            IRepository<TraderDbContext, TraderSymbol, Guid> symbolRepository,
            IRepository<TraderDbContext, ExecutionLog, Guid> logRepository,
            IUnitOfWork<TraderDbContext> uow,
            IBrokerClientFactory brokerFactory,
            ISecretProtector protector,
            IServerClockQueryService clock,
            ILogger<PlanExecutor> logger)
        {
            _planRepository = planRepository;
            _accountRepository = accountRepository;
            _symbolRepository = symbolRepository;
            _logRepository = logRepository;
            _uow = uow;
            _brokerFactory = brokerFactory;
            _protector = protector;
            _clock = clock;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid planId, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId, p => p.Orders)
                ?? throw new Exception($"Plan {planId} not found");

            if (!plan.Enabled) return;

            var planDate = plan.Date;
            var loginAt = new DateTimeOffset(
                planDate.ToDateTime(plan.AutoLoginAt), IranOffset);
            var refreshAt = new DateTimeOffset(
                planDate.ToDateTime(plan.AutoRefreshAt), IranOffset);

            var pendingLogs = new List<PendingLog>();

            try
            {
                /* ─── diff تازه از سرور ─── */
                var clockInfo = await _clock.GetStatusAsync();

                var orderTargets = plan.Orders
                    .OrderBy(o => o.Time)
                    .Select(o => o.Time.ToString("HH:mm:ss.fff"))
                    .ToList();

                var ordersLine = orderTargets.Count > 0
                    ? string.Join(", ", orderTargets)
                    : "—";

                await LogAsync(plan.Id, ExecutionLogLevel.Info,
                    $"Pipeline start | login={loginAt:HH:mm:ss} | refresh={refreshAt:HH:mm:ss} | " +
                    $"orders=[{ordersLine}] | diff={clockInfo.Diff}ms | " +
                    $"latency={clockInfo.OneWayLatency}ms",
                    ct: ct);

                /* ═══ Stage 1: Login check — errors buffered ═══ */
                await EnsureAllAccountsLoggedInAsync(plan, pendingLogs, ct);

                /* ═══ Stage 2: Refresh ═══ */
                await PreciseDelay.UntilAsync(refreshAt, ct);

                /* ═══ Stage 3: Build groups ═══ */
                var groups = plan.Orders
                    .OrderBy(o => o.Time)
                    .GroupBy(o => o.Time)
                    .OrderBy(g => g.Key)
                    .Select(g => new FireGroup
                    {
                        TargetTime = new DateTimeOffset(
                            planDate.ToDateTime(g.Key), IranOffset),
                        Orders = g.ToList(),
                    })
                    .ToList();

                /* ═══ Stage 4: PREPARE (Phase 1) — سری، بدون لاگ ═══ */
                var accountIds = plan.Orders
                    .Select(o => o.AccountId).Distinct().ToList();

                var allAccounts = (await _accountRepository.GetAllAsync())
                    .Where(a => accountIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a);

                var allResults = new List<FireResult>();

                foreach (var group in groups)
                {
                    foreach (var order in group.Orders)
                    {
                        var item = await PrepareFireItemAsync(
                            plan, order, allAccounts, pendingLogs, ct);

                        if (item is not null)
                        {
                            group.Prepared.Add(item);
                        }
                        else
                        {
                            allResults.Add(new FireResult
                            {
                                Order = order,
                                SymbolIsin = order.SymbolIsin,
                                TargetUnixMs = group.TargetTime.ToUnixTimeMilliseconds(),
                                PrepareError = "prepare failed",
                            });
                        }
                    }
                }

                /* ═══ Stage 5: FIRE (Phase 2) — فقط PreciseDelay + HTTP ═══ */
                foreach (var group in groups)
                {
                    if (ct.IsCancellationRequested) break;
                    if (group.Prepared.Count == 0) continue;

                    clockInfo = await _clock.GetStatusAsync();
                    var adjustedTarget = group.TargetTime
                        .AddMilliseconds(-clockInfo.Diff);

                    var lateByMs = (DateTimeOffset.UtcNow - adjustedTarget)
                        .TotalMilliseconds;

                    if (lateByMs > MAX_LATE_MS)
                    {
                        foreach (var p in group.Prepared)
                        {
                            allResults.Add(new FireResult
                            {
                                Order = p.Order,
                                Account = p.Account,
                                SymbolIsin = p.SymbolIsin,
                                Price = p.Price,
                                Quantity = p.Quantity,
                                TargetUnixMs = group.TargetTime.ToUnixTimeMilliseconds(),
                                Exception = new Exception(
                                    $"Cancelled: {lateByMs:F0}ms late"),
                            });
                        }
                        continue;
                    }

                    if (lateByMs <= 0)
                    {
                        await PreciseDelay.UntilAsync(adjustedTarget, ct);
                    }

                    /* ✅ Fire موازی — بدون لاگ */
                    var tasks = group.Prepared
     .Select(p => FireHttpAsync(p, group.TargetTime, clockInfo.Diff, ct))
     .ToList();

                    var results = await Task.WhenAll(tasks);
                    allResults.AddRange(results);
                }

                /* ═══ Stage 6: SAVE + LOG (Phase 3) — همه‌چیز یک‌جا ═══ */
                foreach (var log in pendingLogs)
                {
                    await LogAsync(plan.Id, log.Level, log.Message,
                        log.OrderId, log.Code, ct);
                }

                foreach (var r in allResults)
                {
                    if (ct.IsCancellationRequested) break;
                    await SaveAndLogFireResultAsync(plan, r, ct);
                }

                plan.SetStatus(SchedulePlanStatus.Done, "All orders fired");
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException)
            {
                plan.SetStatus(SchedulePlanStatus.Cancelled, "Cancelled");
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                plan.SetStatus(SchedulePlanStatus.Error, ex.Message);
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(CancellationToken.None);

                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"Pipeline error: {ex.Message}", ct: CancellationToken.None);

                throw;
            }
        }

        /* ══════════════════════════════════════════════
           Phase 1 — Prepare (سری، بدون لاگ DB)
           ══════════════════════════════════════════════ */
        private async Task<PreparedFireItem?> PrepareFireItemAsync(
            SchedulePlan plan,
            ScheduledOrder order,
            Dictionary<Guid, TraderAccount> allAccounts,
            List<PendingLog> pendingLogs,
            CancellationToken ct)
        {
            if (!allAccounts.TryGetValue(order.AccountId, out var account))
            {
                pendingLogs.Add(new PendingLog(
                    ExecutionLogLevel.Error,
                    $"Account {order.AccountId} not found",
                    order.Id));
                return null;
            }

            if (account.GetSessionStatus() != SessionStatus.Valid)
            {
                pendingLogs.Add(new PendingLog(
                    ExecutionLogLevel.Error,
                    $"[{account.Name}] {order.SymbolIsin}: session invalid",
                    order.Id));
                return null;
            }

            var brokerClient = _brokerFactory.GetClient(account.Broker);
            var sessionJson = _protector.Unprotect(account.EncryptedSession!);
            var session = brokerClient.DeserializeSession(sessionJson);

            SymbolMarketDataDto marketData;
            try
            {
                marketData = await brokerClient.GetSymbolInfoAsync(
                    session, order.SymbolIsin, ct);
            }
            catch (Exception ex)
            {
                pendingLogs.Add(new PendingLog(
                    ExecutionLogLevel.Error,
                    $"[{account.Name}] {order.SymbolIsin}: symbol info failed: {ex.Message}",
                    order.Id));
                return null;
            }

            var price = order.Side == OrderSide.Buy
                ? marketData.HighAllowedPrice
                : marketData.LowAllowedPrice;

            if (price is null || price <= 0)
            {
                pendingLogs.Add(new PendingLog(
                    ExecutionLogLevel.Error,
                    $"[{account.Name}] {order.SymbolIsin}: no valid allowed price",
                    order.Id));
                return null;
            }

            long quantity;
            if (order.Mode == OrderMode.Quantity)
            {
                quantity = order.Quantity;
            }
            else
            {
                const double commission = 0.0037;
                quantity = (long)Math.Floor(
                    order.TotalValue / ((double)price.Value * (1 + commission)));
            }

            if (quantity <= 0)
            {
                pendingLogs.Add(new PendingLog(
                    ExecutionLogLevel.Error,
                    $"[{account.Name}] {order.SymbolIsin}: invalid quantity ({quantity})",
                    order.Id));
                return null;
            }

            var httpRequest = brokerClient.BuildOrderRequest(
                session,
                order.SymbolIsin,
                price.Value,
                quantity,
                order.Side == OrderSide.Buy ? 0 : 1);

            return new PreparedFireItem
            {
                Order = order,
                Account = account,
                BrokerClient = brokerClient,
                Session = session,
                SymbolIsin = order.SymbolIsin,
                Price = price.Value,
                Quantity = quantity,
                HttpRequest = httpRequest,
            };
        }

        /* ══════════════════════════════════════════════
           Phase 2 — Fire (موازی، بدون لاگ)
           ══════════════════════════════════════════════ */
        private async Task<FireResult> FireHttpAsync(
    PreparedFireItem item,
    DateTimeOffset target,
    long clockDiffUsed,
    CancellationToken ct)
        {
            try
            {
                var result = await item.BrokerClient.SendOrderWithRequestAsync(
                    item.Session,
                    item.HttpRequest,
                    item.SymbolIsin,
                    ct);

                return new FireResult
                {
                    Order = item.Order,
                    Account = item.Account,
                    SymbolIsin = item.SymbolIsin,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    TargetUnixMs = target.ToUnixTimeMilliseconds(),
                    ClockDiffUsed = clockDiffUsed,
                    Result = result,
                };
            }
            catch (EasyTraderOrderException ex)
            {
                return new FireResult
                {
                    Order = item.Order,
                    Account = item.Account,
                    SymbolIsin = item.SymbolIsin,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    TargetUnixMs = target.ToUnixTimeMilliseconds(),
                    ClockDiffUsed = clockDiffUsed,
                    Exception = ex,
                    FireAtUnixMs = ex.FireAtUnixMs,
                    ReceivedAtUnixMs = ex.ReceivedAtUnixMs,
                };
            }
            catch (Exception ex)
            {
                return new FireResult
                {
                    Order = item.Order,
                    Account = item.Account,
                    SymbolIsin = item.SymbolIsin,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    TargetUnixMs = target.ToUnixTimeMilliseconds(),
                    ClockDiffUsed = clockDiffUsed,
                    Exception = ex,
                };
            }
        }

        /* ══════════════════════════════════════════════
        Phase 3 — Save + Log (یک‌جا)
        ══════════════════════════════════════════════ */

        private async Task SaveAndLogFireResultAsync(
            SchedulePlan plan,
            FireResult r,
            CancellationToken ct)
        {
            /* ─── prepare failure ─── */
            if (r.PrepareError is not null)
            {
                r.Order.MarkFired(r.PrepareError);
                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"❌ {r.SymbolIsin} | {r.PrepareError}",
                    orderId: r.Order.Id, ct: ct);
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(ct);
                return;
            }

            /* ─── fire error (با timings) ─── */
            if (r.Exception is not null)
            {
                var delta = r.FireAtUnixMs > 0
                    ? r.FireAtUnixMs - r.TargetUnixMs
                    : (long?)null;

                r.Order.MarkFired($"ERROR: {r.Exception.Message}");

                var deltaStr = delta.HasValue
                    ? $"Δ={delta.Value:+#;-#;0}ms | "
                    : "";

                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"❌ [{r.Account?.Name}] {r.SymbolIsin} | " +
                    $"target={FmtUnix(r.TargetUnixMs)} ({r.TargetUnixMs}) | " +
                    deltaStr +
                    r.Exception.Message,
                    orderId: r.Order.Id, ct: ct);

                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(ct);
                return;
            }

            /* ─── success یا خطای منطقی ─── */
            var res = r.Result!;
            var deltaMs = res.FireAtUnixMs - r.TargetUnixMs;
            var rttMs = res.ReceivedAtUnixMs - res.FireAtUnixMs;

            if (res.IsSuccessful)
            {
                r.Order.MarkFired(res.OrderId);
                await LogAsync(plan.Id, ExecutionLogLevel.Success,
                    $"✅ [{r.Account!.Name}] {r.SymbolIsin} {r.Price}×{r.Quantity} | " +
                   $"target={FmtUnix(r.TargetUnixMs)} ({r.TargetUnixMs}) | " +
                    $"fire={FmtUnix(res.FireAtUnixMs)} ({res.FireAtUnixMs}) | " +
                    $"Δ={deltaMs:+#;-#;0}ms | " +
                    $"diff={r.ClockDiffUsed}ms | " +
                    $"rtt={rttMs}ms | " +
                    $"id={res.OrderId}",
                    orderId: r.Order.Id, ct: ct);
            }
            else
            {
                r.Order.MarkFired(res.Message);
                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"❌ [{r.Account!.Name}] {r.SymbolIsin} | " +
                    $"target={FmtUnix(r.TargetUnixMs)} ({r.TargetUnixMs}) | " +
                    $"fire={FmtUnix(res.FireAtUnixMs)} ({res.FireAtUnixMs}) | " +
                    $"Δ={deltaMs:+#;-#;0}ms | " +
                    $"rtt={rttMs}ms | " +
                    $"code={res.ErrorCode} | " +
                    $"{res.Message}",
                    orderId: r.Order.Id, code: res.ErrorCode, ct: ct);
            }

            await _planRepository.UpdateAsync(plan);
            await _uow.SaveChangesAsync(ct);
        }
        /// <summary>Unix ms → "HH:mm:ss.fff" به وقت ایران</summary>
        private static string FmtUnix(long unixMs)
        {
            if (unixMs <= 0) return "—";
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(unixMs)
                .ToOffset(IranOffset);
            return dt.ToString("HH:mm:ss.fff");
        }

        /* ═══════════════════ Other Stages ═══════════════════ */

        private async Task EnsureAllAccountsLoggedInAsync(
            SchedulePlan plan,
            List<PendingLog> pendingLogs,
            CancellationToken ct)
        {
            var accountIds = plan.Orders
                .Select(o => o.AccountId).Distinct().ToList();

            var accounts = (await _accountRepository.GetAllAsync())
                .Where(a => accountIds.Contains(a.Id))
                .ToList();

            foreach (var account in accounts)
            {
                if (account.GetSessionStatus() == SessionStatus.Valid) continue;

                pendingLogs.Add(new PendingLog(
                    ExecutionLogLevel.Warning,
                    $"Account {account.Name}: session invalid",
                    null));
            }

            await Task.CompletedTask;
        }

        private async Task LogAsync(
            Guid planId,
            ExecutionLogLevel level,
            string message,
            Guid? orderId = null,
            int? code = null,
            CancellationToken ct = default)
        {
            var log = ExecutionLog.Create(planId, level, message, orderId, code);
            await _logRepository.AddAsync(log);
            await _uow.SaveChangesAsync(ct);
        }

        /* ═══ Helper types ═══ */
        private class FireGroup
        {
            public DateTimeOffset TargetTime { get; set; }
            public List<ScheduledOrder> Orders { get; set; } = new();
            public List<PreparedFireItem> Prepared { get; set; } = new();
        }

        private class PreparedFireItem
        {
            public ScheduledOrder Order { get; set; } = default!;
            public TraderAccount Account { get; set; } = default!;
            public IBrokerClient BrokerClient { get; set; } = default!;
            public BrokerSession Session { get; set; } = default!;
            public string SymbolIsin { get; set; } = default!;
            public long Price { get; set; }
            public long Quantity { get; set; }
            public HttpRequestMessage HttpRequest { get; set; } = default!;
        }

        private class FireResult
        {
            public ScheduledOrder Order { get; set; } = default!;
            public TraderAccount? Account { get; set; }
            public string SymbolIsin { get; set; } = default!;
            public long Price { get; set; }
            public long Quantity { get; set; }
            public long TargetUnixMs { get; set; }
            public long ClockDiffUsed { get; set; }
            public BrokerOrderResultDto? Result { get; set; }
            public Exception? Exception { get; set; }
            public string? PrepareError { get; set; }
            public long FireAtUnixMs { get; set; }
            public long ReceivedAtUnixMs { get; set; }
        }

        private record PendingLog(
            ExecutionLogLevel Level,
            string Message,
            Guid? OrderId,
            int? Code = null);
    }
}