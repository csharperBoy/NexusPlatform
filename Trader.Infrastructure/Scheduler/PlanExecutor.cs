using Core.Application.Abstractions;
using Core.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;
using Scheduler.Infrastructure.Jobs;
using Trader.Application.Abstractions;
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
        private readonly IRepository<TraderDbContext, ExecutionLog, Guid> _logRepository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        private readonly IBrokerClientFactory _brokerFactory;
        private readonly IServerClockQueryService _clock;
        private readonly ISecretProtector _protector;
        private readonly ILogger<PlanExecutor> _logger;

        private static readonly TimeSpan IranOffset = TimeSpan.FromHours(3.5);
        private const long MAX_LATE_MS = 5000;
        private const long FIRE_TOLERANCE_MS = 5;

        public PlanExecutor(
            IRepository<TraderDbContext, SchedulePlan, Guid> planRepository,
            IRepository<TraderDbContext, TraderAccount, Guid> accountRepository,
            IRepository<TraderDbContext, ExecutionLog, Guid> logRepository,
            IUnitOfWork<TraderDbContext> uow,
            IBrokerClientFactory brokerFactory,
            IServerClockQueryService clock,
            ISecretProtector protector,
            ILogger<PlanExecutor> logger)
        {
            _planRepository = planRepository;
            _accountRepository = accountRepository;
            _logRepository = logRepository;
            _uow = uow;
            _brokerFactory = brokerFactory;
            _clock = clock;
            _protector = protector;
            _logger = logger;
        }

        public async Task ExecuteAsync(Guid planId, CancellationToken ct = default)
        {
            var plan = await _planRepository.GetByIdAsync(planId)
                ?? throw new Exception($"Plan {planId} not found");

            if (!plan.Enabled)
            {
                _logger.LogWarning("Plan {PlanId} is disabled, skipping", planId);
                return;
            }

            /* ─── Compute absolute times ─── */
            var planDate = plan.Date;
            var loginAt = new DateTimeOffset(
                planDate.ToDateTime(plan.AutoLoginAt), IranOffset);
            var refreshAt = new DateTimeOffset(
                planDate.ToDateTime(plan.AutoRefreshAt), IranOffset);

            var now = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Plan {PlanId} ({Name}) starting pipeline: login={Login} refresh={Refresh} orders={Count}",
                planId, plan.Name, loginAt, refreshAt, plan.Orders.Count);

            try
            {
                /* ═══ Stage 1: Login (بدون PreciseDelay — معمولاً Hangfire ~۱۵s قبل می‌زنه) ═══ */
                await LogAsync(plan.Id, ExecutionLogLevel.Info,
                    $"Pipeline start (login={loginAt:HH:mm:ss}, refresh={refreshAt:HH:mm:ss})",
                    ct: ct);

                // اینجا login انجام می‌شه ولی چون دیگه جداگانه نیست، فقط چک می‌کنیم که session معتبره
                await EnsureAllAccountsLoggedInAsync(plan, ct);

                /* ═══ Stage 2: Wait until refresh time, then refresh prices ═══ */
                await PreciseDelay.UntilAsync(refreshAt, ct);
                await RefreshSymbolsAsync(plan, ct);

                /* ═══ Stage 3: Group orders by time and fire ═══ */
                var groups = plan.Orders
                    .OrderBy(o => o.Time)
                    .GroupBy(o => o.Time)
                    .OrderBy(g => g.Key)
                    .ToList();

                var firedOrders = new List<ScheduledOrder>();

                foreach (var group in groups)
                {
                    if (ct.IsCancellationRequested) break;

                    var orderTime = new DateTimeOffset(
                        planDate.ToDateTime(group.Key), IranOffset);

                    // چک دیر شدن
                    if (DateTimeOffset.UtcNow - orderTime > TimeSpan.FromMilliseconds(MAX_LATE_MS))
                    {
                        foreach (var o in group)
                        {
                            await LogAsync(plan.Id, ExecutionLogLevel.Warning,
                                $"Order {o.SymbolIsin} cancelled — past {orderTime:HH:mm:ss.fff}",
                                orderId: o.Id, ct: ct);
                        }
                        continue;
                    }

                    // PreciseDelay تا دقیقاً سر order time
                    await PreciseDelay.UntilAsync(orderTime, ct);

                    // Fire همه سفارش‌های هم‌زمان موازی
                    var tasks = group.Select(o =>
                        FireSingleOrderAsync(plan, o, ct)).ToList();

                    await Task.WhenAll(tasks);
                    firedOrders.AddRange(group);
                }

                plan.SetStatus(SchedulePlanStatus.Done, "All orders fired");
                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Plan {PlanId} completed. Fired {Count} orders",
                    planId, firedOrders.Count);
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

        /* ═══════════════════ Stages ═══════════════════ */

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

            // توی این نسخه، symbol info از API لحظه‌ای خونده می‌شه
            // (قبلاً توی فرانت ساختیم، اینجا backend خودش می‌خونه)
            // فعلاً فقط log — چون symbol info cache مشترک هنوز نساختیم.
            await Task.CompletedTask;
        }

        private async Task FireSingleOrderAsync(
            SchedulePlan plan, ScheduledOrder order, CancellationToken ct)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(order.AccountId)
                    ?? throw new Exception($"Account {order.AccountId} not found");

                if (account.GetSessionStatus() != SessionStatus.Valid)
                    throw new Exception($"Account {account.Name}: invalid session");

                var brokerClient = _brokerFactory.GetClient(account.Broker);
                var sessionJson = _protector.Unprotect(account.EncryptedSession!);
                var session = brokerClient.DeserializeSession(sessionJson);

                /* ─── پیدا کردن symbol برای name/isin ─── */
                var symbolName = order.SymbolIsin;   // فعلاً فرض name == isin
                // TODO: جستجو در symbolRepository

                /* ─── قیمت از cache لحظه‌ای (server-time info) ─── */
                var marketData = await brokerClient.GetSymbolInfoAsync(
                    session, symbolName, ct);

                var price = order.Side == OrderSide.Buy
                    ? marketData.HighAllowedPrice
                    : marketData.LowAllowedPrice;

                if (price is null || price <= 0)
                    throw new Exception("No valid allowed price");

                /* ─── محاسبه quantity ─── */
                long quantity;
                if (order.Mode == OrderMode.Quantity)
                {
                    quantity = order.Quantity;
                }
                else
                {
                    // totalValue / (price * (1 + commission))
                    // commission فعلاً ثابت 0.0037
                    quantity = (long)Math.Floor(
                        order.TotalValue / ((double)price * (1 + 0.0037)));
                }

                if (quantity <= 0)
                    throw new Exception($"Invalid quantity: {quantity}");

                /* ─── Fire ─── */
                var result = order.Side == OrderSide.Buy
                    ? await brokerClient.SendBuyOrderAsync(
                        session, symbolName, price.Value, quantity, ct)
                    : await brokerClient.SendSellOrderAsync(
                        session, symbolName, price.Value, quantity, ct);

                if (result.IsSuccessful)
                {
                    order.MarkFired(result.OrderId);
                    await LogAsync(plan.Id, ExecutionLogLevel.Success,
                        $"[{account.Name}] {symbolName} {price}×{quantity} fired (id={result.OrderId})",
                        orderId: order.Id, ct: ct);
                }
                else
                {
                    order.MarkFired(result.Message);
                    await LogAsync(plan.Id, ExecutionLogLevel.Error,
                        $"[{account.Name}] {symbolName} failed: {result.ErrorCode} {result.Message}",
                        orderId: order.Id, code: result.ErrorCode, ct: ct);
                }

                await _planRepository.UpdateAsync(plan);
                await _uow.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                await LogAsync(plan.Id, ExecutionLogLevel.Error,
                    $"Fire error {order.SymbolIsin}: {ex.Message}",
                    orderId: order.Id, ct: CancellationToken.None);
            }
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
    }
}