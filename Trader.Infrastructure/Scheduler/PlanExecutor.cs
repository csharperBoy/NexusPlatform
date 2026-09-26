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
            ILogger<PlanExecutor> logger)
        {
            _planRepository = planRepository;
            _accountRepository = accountRepository;
            _symbolRepository = symbolRepository;
            _logRepository = logRepository;
            _uow = uow;
            _brokerFactory = brokerFactory;
            _protector = protector;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid planId, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId, p => p.Orders)
                ?? throw new Exception($"Plan {planId} not found");

            if (!plan.Enabled)
            {
                _logger.LogWarning("Plan {PlanId} is disabled, skipping", planId);
                return;
            }

            var planDate = plan.Date;
            var loginAt = new DateTimeOffset(
                planDate.ToDateTime(plan.AutoLoginAt), IranOffset);
            var refreshAt = new DateTimeOffset(
                planDate.ToDateTime(plan.AutoRefreshAt), IranOffset);

            _logger.LogInformation(
                "Plan {PlanId} ({Name}) starting pipeline: login={Login} refresh={Refresh} orders={Count}",
                planId, plan.Name, loginAt, refreshAt, plan.Orders.Count);

            try
            {
                await LogAsync(plan.Id, ExecutionLogLevel.Info,
                    $"Pipeline start (login={loginAt:HH:mm:ss}, refresh={refreshAt:HH:mm:ss})",
                    ct: ct);

                /* ═══ Stage 1: Login check ═══ */
                await EnsureAllAccountsLoggedInAsync(plan, ct);

                /* ═══ Stage 2: Wait until refresh time, then refresh ═══ */
                await PreciseDelay.UntilAsync(refreshAt, ct);
                await RefreshSymbolsAsync(plan, ct);

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

                /* ═══ Stage 4: PREPARE ALL GROUPS (Phase 1) — قبل از هر PreciseDelay ═══ */
                /* این تضمین می‌کنه که سر لحظه‌ی fire، هیچ DB read ای نداریم */

                // یک‌بار همه‌ی symbolها رو بگیر
                var allSymbols = (await _symbolRepository.GetAllAsync())
                    .ToDictionary(s => s.SymbolIsin, s => s.SymbolName);

                // یک‌بار همه‌ی accountهای لازم رو بگیر
                var accountIds = plan.Orders.Select(o => o.AccountId).Distinct().ToList();
                var allAccounts = (await _accountRepository.GetAllAsync())
                    .Where(a => accountIds.Contains(a.Id))
                    .ToDictionary(a => a.Id, a => a);

                foreach (var group in groups)
                {
                    foreach (var order in group.Orders)
                    {
                        var item = await PrepareFireItemAsync(
                            plan, order, allAccounts, allSymbols, ct);
                        if (item is not null)
                            group.Prepared.Add(item);
                    }
                }

                _logger.LogInformation(
                    "Plan {PlanId}: prepared {Ready}/{Total} orders",
                    planId,
                    groups.Sum(g => g.Prepared.Count),
                    plan.Orders.Count);

                /* ═══ Stage 5: FIRE — فقط PreciseDelay + HTTP (بدون DB) ═══ */
                var allResults = new List<FireResult>();

                foreach (var group in groups)
                {
                    if (ct.IsCancellationRequested) break;
                    if (group.Prepared.Count == 0) continue;

                    /* دیر شده؟ */
                    var lateBy = DateTimeOffset.UtcNow - group.TargetTime;
                    if (lateBy > TimeSpan.FromMilliseconds(MAX_LATE_MS))
                    {
                        foreach (var p in group.Prepared)
                        {
                            allResults.Add(new FireResult
                            {
                                Order = p.Order,
                                Account = p.Account,
                                Exception = new Exception(
                                    $"Cancelled — past {group.TargetTime:HH:mm:ss.fff}"),
                            });
                        }
                        continue;
                    }

                    /* PreciseDelay دقیقاً سر لحظه‌ی هدف */
                    await PreciseDelay.UntilAsync(group.TargetTime, ct);

                    /* Fire موازی — HTTP only */
                    var tasks = group.Prepared
                        .Select(p => FireHttpAsync(p, ct))
                        .ToList();

                    var results = await Task.WhenAll(tasks);
                    allResults.AddRange(results);
                }

                /* ═══ Stage 6: SAVE ALL RESULTS (Phase 3) — یک‌جا در آخر ═══ */
                foreach (var r in allResults)
                {
                    if (ct.IsCancellationRequested) break;
                    await SaveFireResultAsync(plan, r, ct);
                }

                plan.SetStatus(SchedulePlanStatus.Done, "All orders fired");
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Plan {PlanId} completed. Fired {Count} orders",
                    planId, plan.Orders.Count(o => o.Fired));
            }
            catch (OperationCanceledException)
            {
                plan.SetStatus(SchedulePlanStatus.Cancelled, "Cancelled");
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(CancellationToken.None);
                _logger.LogWarning("Plan {PlanId} cancelled", planId);
            }
            catch (Exception ex)
            {
                plan.SetStatus(SchedulePlanStatus.Error, ex.Message);
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(CancellationToken.None);

                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"Pipeline error: {ex.Message}", ct: CancellationToken.None);

                _logger.LogError(ex, "Plan {PlanId} failed", planId);
                throw;
            }
        }

        /* ══════════════════════════════════════════════
           Phase 1 — Prepare (سری، با parent DbContext)
           ══════════════════════════════════════════════ */
        private async Task<PreparedFireItem?> PrepareFireItemAsync(
                                                    SchedulePlan plan,
                                                    ScheduledOrder order,
                                                    Dictionary<Guid, TraderAccount> allAccounts,
                                                    Dictionary<string, string> allSymbols,
                                                    CancellationToken ct)
        {
            if (!allAccounts.TryGetValue(order.AccountId, out var account))
            {
                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"Account {order.AccountId} not found",
                    orderId: order.Id, ct: ct);
                return null;
            }

            if (account.GetSessionStatus() != SessionStatus.Valid)
            {
                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"[{account.Name}] {order.SymbolIsin}: session invalid",
                    orderId: order.Id, ct: ct);
                return null;
            }

            var brokerClient = _brokerFactory.GetClient(account.Broker);
            var sessionJson = _protector.Unprotect(account.EncryptedSession!);
            var session = brokerClient.DeserializeSession(sessionJson);

            var symbolName = allSymbols.GetValueOrDefault(order.SymbolIsin, order.SymbolIsin);

            return await Task.FromResult(new PreparedFireItem
            {
                Order = order,
                Account = account,
                BrokerClient = brokerClient,
                Session = session,
                SymbolName = symbolName,
            });
        }

        /* ══════════════════════════════════════════════
           Phase 2 — Fire (موازی، بدون DB)
           ══════════════════════════════════════════════ */
        private async Task<FireResult> FireHttpAsync(
            PreparedFireItem item,
            CancellationToken ct)
        {
            try
            {
                var marketData = await item.BrokerClient.GetSymbolInfoAsync(
                    item.Session, item.SymbolName, ct);

                var price = item.Order.Side == OrderSide.Buy
                    ? marketData.HighAllowedPrice
                    : marketData.LowAllowedPrice;

                if (price is null || price <= 0)
                    throw new Exception($"No valid allowed price for {item.SymbolName}");

                long quantity;
                if (item.Order.Mode == OrderMode.Quantity)
                {
                    quantity = item.Order.Quantity;
                }
                else
                {
                    const double commission = 0.0037;
                    quantity = (long)Math.Floor(
                        item.Order.TotalValue / ((double)price.Value * (1 + commission)));
                }

                if (quantity <= 0)
                    throw new Exception($"Invalid quantity: {quantity}");

                var result = item.Order.Side == OrderSide.Buy
                    ? await item.BrokerClient.SendBuyOrderAsync(
                        item.Session, item.SymbolName, price.Value, quantity, ct)
                    : await item.BrokerClient.SendSellOrderAsync(
                        item.Session, item.SymbolName, price.Value, quantity, ct);

                return new FireResult
                {
                    Order = item.Order,
                    Account = item.Account,
                    Result = result,
                    Price = price.Value,
                    Quantity = quantity,
                };
            }
            catch (Exception ex)
            {
                return new FireResult
                {
                    Order = item.Order,
                    Account = item.Account,
                    Exception = ex,
                };
            }
        }

        /* ══════════════════════════════════════════════
           Phase 3 — Save (سری، با parent DbContext)
           ══════════════════════════════════════════════ */
        private async Task SaveFireResultAsync(
            SchedulePlan plan,
            FireResult r,
            CancellationToken ct)
        {
            var symName = r.Order.SymbolIsin;

            if (r.Exception is not null)
            {
                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"Fire error {symName}: {r.Exception.Message}",
                    orderId: r.Order.Id, ct: ct);
                r.Order.MarkFired($"ERROR: {r.Exception.Message}");
            }
            else if (r.Result!.IsSuccessful)
            {
                r.Order.MarkFired(r.Result.OrderId);
                await LogAsync(plan.Id, ExecutionLogLevel.Success,
                    $"[{r.Account.Name}] {symName} {r.Price}×{r.Quantity} fired (id={r.Result.OrderId})",
                    orderId: r.Order.Id, ct: ct);
            }
            else
            {
                r.Order.MarkFired(r.Result.Message);
                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"[{r.Account.Name}] {symName} failed: {r.Result.ErrorCode} {r.Result.Message}",
                    orderId: r.Order.Id, code: r.Result.ErrorCode, ct: ct);
            }

            await _planRepository.UpdateAsync(plan);
            await _uow.SaveChangesAsync(ct);
        }

        /* ═══════════════════ Other Stages ═══════════════════ */

        private async Task EnsureAllAccountsLoggedInAsync(
            SchedulePlan plan, CancellationToken ct)
        {
            var accountIds = plan.Orders.Select(o => o.AccountId).Distinct().ToList();
            var accounts = (await _accountRepository.GetAllAsync())
                .Where(a => accountIds.Contains(a.Id))
                .ToList();

            foreach (var account in accounts)
            {
                if (account.GetSessionStatus() == SessionStatus.Valid) continue;

                await LogAsync(plan.Id, ExecutionLogLevel.Warning,
                    $"Account {account.Name}: session invalid, requires manual login",
                    ct: ct);
            }
        }

        private async Task RefreshSymbolsAsync(SchedulePlan plan, CancellationToken ct)
        {
            var isins = plan.Orders.Select(o => o.SymbolIsin).Distinct().ToList();
            await LogAsync(plan.Id, ExecutionLogLevel.Info,
                $"Refreshing {isins.Count} symbols", ct: ct);
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

            _logger.Log(
                level == ExecutionLogLevel.Error ? LogLevel.Error :
                level == ExecutionLogLevel.Warning ? LogLevel.Warning :
                LogLevel.Information,
                "Plan {PlanId} [{Level}] {Message}",
                planId, level, message);
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
            public string SymbolName { get; set; } = default!;
        }

        private class FireResult
        {
            public ScheduledOrder Order { get; set; } = default!;
            public TraderAccount Account { get; set; } = default!;
            public BrokerOrderResultDto? Result { get; set; }
            public Exception? Exception { get; set; }
            public long Price { get; set; }
            public long Quantity { get; set; }
        }
    }
}