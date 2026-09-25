using Core.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Application.Abstractions;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;
using Trader.Application.Scheduler;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services
{
    public class SchedulePlanService : ISchedulePlanCommandService, ISchedulePlanQueryService
    {
        private readonly IRepository<TraderDbContext, SchedulePlan, Guid> _planRepository;
        private readonly IRepository<TraderDbContext, TraderAccount, Guid> _accountRepository;
        private readonly IRepository<TraderDbContext, TraderSymbol, Guid> _symbolRepository;
        private readonly IUnitOfWork<TraderDbContext> _uow;
        private readonly ISchedulerService _scheduler;
        private readonly ILogger<SchedulePlanService> _logger;

        // Iran Standard Time: +03:30 (بدون DST)
        private static readonly TimeSpan IranOffset = TimeSpan.FromHours(3.5);

        public SchedulePlanService(
            IRepository<TraderDbContext, SchedulePlan, Guid> planRepository,
            IRepository<TraderDbContext, TraderAccount, Guid> accountRepository,
            IRepository<TraderDbContext, TraderSymbol, Guid> symbolRepository,
            IUnitOfWork<TraderDbContext> uow,
            ISchedulerService scheduler,
            ILogger<SchedulePlanService> logger)
        {
            _planRepository = planRepository;
            _accountRepository = accountRepository;
            _symbolRepository = symbolRepository;
            _uow = uow;
            _scheduler = scheduler;
            _logger = logger;
        }

        /* ═══════════════════ Commands ═══════════════════ */

        public async Task<Guid> CreateSchedulePlanAsync(
            string name,
            string date,
            bool enabled,
            string autoLoginAt,
            string autoRefreshAt,
            List<ScheduledOrderItemDto> orders)
        {
            var (planDate, loginTime, refreshTime) = ParsePlanInputs(
                date, autoLoginAt, autoRefreshAt);

            var plan = SchedulePlan.Create(
                name, planDate, enabled, loginTime, refreshTime);

            await ApplyOrdersAsync(plan, orders);

            await _planRepository.AddAsync(plan);

            _logger.LogInformation(
                "Created SchedulePlan {Id} ({Name}) date={Date} enabled={Enabled}",
                plan.Id, name, planDate, enabled);

            return plan.Id;
        }

        public async Task<Guid> UpdateSchedulePlanAsync(
            Guid id,
            string name,
            string date,
            bool enabled,
            string autoLoginAt,
            string autoRefreshAt,
            List<ScheduledOrderItemDto> orders)
        {
            var plan = await _planRepository.GetByIdAsync(id)
                ?? throw new Exception($"SchedulePlan {id} not found");

            var wasEnabled = plan.Enabled;

            var (planDate, loginTime, refreshTime) = ParsePlanInputs(
                date, autoLoginAt, autoRefreshAt);

            plan.SetInfo(name, planDate, enabled, loginTime, refreshTime);

            await ApplyOrdersAsync(plan, orders, replace: true);

            await _planRepository.UpdateAsync(plan);

            /* ─── اگه پلن disable شد، job رو cancel کن ─── */
            if (wasEnabled && !enabled)
            {
                await CancelScheduledJobAsync(plan.Id);
            }

            _logger.LogInformation(
                "Updated SchedulePlan {Id} ({Name}) enabled={Enabled}",
                plan.Id, name, enabled);

            return plan.Id;
        }

        public async Task<bool> DeleteSchedulePlanAsync(Guid id)
        {
            var plan = await _planRepository.GetByIdAsync(id)
                ?? throw new Exception($"SchedulePlan {id} not found");

            if (plan.Enabled)
                await CancelScheduledJobAsync(plan.Id);

            await _planRepository.DeleteAsync(plan);

            _logger.LogInformation("Deleted SchedulePlan {Id}", id);

            return true;
        }

        public async Task EnableAsync(Guid id)
        {
            var plan = await _planRepository.GetByIdAsync(id)
                ?? throw new Exception($"SchedulePlan {id} not found");

            if (plan.Enabled)
            {
                _logger.LogDebug("SchedulePlan {Id} already enabled", id);
                return;
            }

            plan.Enable();
            await _planRepository.UpdateAsync(plan);

            /* ─── زمان‌بندی job ─── */
            await ScheduleJobAsync(plan);

            _logger.LogInformation("Enabled SchedulePlan {Id}", id);
        }

        public async Task DisableAsync(Guid id)
        {
            var plan = await _planRepository.GetByIdAsync(id)
                ?? throw new Exception($"SchedulePlan {id} not found");

            if (!plan.Enabled)
            {
                _logger.LogDebug("SchedulePlan {Id} already disabled", id);
                return;
            }

            plan.Disable();
            await _planRepository.UpdateAsync(plan);

            await CancelScheduledJobAsync(plan.Id);

            _logger.LogInformation("Disabled SchedulePlan {Id}", id);
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }

        /* ═══════════════════ Queries ═══════════════════ */

        public async Task<IReadOnlyList<SchedulePlanInfoView>> GetSchedulePlanListAsync()
        {
            var plans = await _planRepository.GetAllAsync(
                queryOptions: q => q.OrderBy(p => p.Date).ThenBy(p => p.Name));

            var accounts = (await _accountRepository.GetAllAsync())
                .ToDictionary(a => a.Id, a => a.Name);

            var symbols = (await _symbolRepository.GetAllAsync())
                .ToDictionary(s => s.SymbolIsin, s => s.SymbolName);

            return plans.Select(p => MapToView(p, accounts, symbols)).ToList();
        }

        public async Task<SchedulePlanInfoView?> GetSchedulePlanByIdAsync(Guid id)
        {
            var plan = await _planRepository.GetByIdAsync(id);
            if (plan is null) return null;

            var accounts = (await _accountRepository.GetAllAsync())
                .ToDictionary(a => a.Id, a => a.Name);

            var symbols = (await _symbolRepository.GetAllAsync())
                .ToDictionary(s => s.SymbolIsin, s => s.SymbolName);

            return MapToView(plan, accounts, symbols);
        }

        /* ═══════════════════ Helpers ═══════════════════ */

        private static (DateOnly Date, TimeOnly Login, TimeOnly Refresh) ParsePlanInputs(
            string date, string login, string refresh)
        {
            if (!DateOnly.TryParse(date, out var d))
                throw new Exception($"Invalid date: {date}");

            if (!TimeOnly.TryParse(login, out var l))
                throw new Exception($"Invalid login time: {login}");

            if (!TimeOnly.TryParse(refresh, out var r))
                throw new Exception($"Invalid refresh time: {refresh}");

            return (d, l, r);
        }

        private async Task ApplyOrdersAsync(
            SchedulePlan plan,
            List<ScheduledOrderItemDto> orders,
            bool replace = false)
        {
            if (replace) plan.ClearOrders();

            /* ─── اعتبارسنجی حساب‌ها و نمادها یک‌بار ─── */
            var accountIds = orders.Select(o => o.AccountId).Distinct().ToList();
            var isins = orders.Select(o => o.SymbolIsin).Distinct().ToList();

            var existingAccounts = (await _accountRepository.GetAllAsync())
                .Where(a => accountIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToHashSet();

            var existingSymbols = (await _symbolRepository.GetAllAsync())
                .Where(s => isins.Contains(s.SymbolIsin))
                .Select(s => s.SymbolIsin)
                .ToHashSet();

            for (int i = 0; i < orders.Count; i++)
            {
                var o = orders[i];

                if (!existingAccounts.Contains(o.AccountId))
                    throw new Exception($"Order #{i + 1}: account {o.AccountId} not found");

                if (!existingSymbols.Contains(o.SymbolIsin))
                    throw new Exception($"Order #{i + 1}: symbol {o.SymbolIsin} not found");

                if (!TimeOnly.TryParse(o.Time, out var orderTime))
                    throw new Exception($"Order #{i + 1}: invalid time '{o.Time}'");

                if (!long.TryParse(o.Quantity, out var qty))
                    throw new Exception($"Order #{i + 1}: invalid quantity '{o.Quantity}'");

                if (!long.TryParse(o.TotalValue, out var totalValue))
                    throw new Exception($"Order #{i + 1}: invalid totalValue '{o.TotalValue}'");

                var mode = o.Mode == "totalValue" ? OrderMode.TotalValue : OrderMode.Quantity;
                var side = o.Side == 1 ? OrderSide.Sell : OrderSide.Buy;

                plan.AddOrder(
                    o.AccountId,
                    o.SymbolIsin,
                    side,
                    mode,
                    qty,
                    totalValue,
                    orderTime);
            }
        }

        private async Task ScheduleJobAsync(SchedulePlan plan)
        {
            var fireAt = ComputeFireAt(plan);

            if (fireAt <= DateTimeOffset.UtcNow)
            {
                _logger.LogWarning(
                    "SchedulePlan {Id} fireAt={FireAt} is in the past, skipping schedule",
                    plan.Id, fireAt);
                return;
            }

            var payload = new PlanExecutionPayload(plan.Id);

            var jobId = await _scheduler.ScheduleAsync(
                payload,
                fireAt,
                queue: "scheduler");

            _logger.LogInformation(
                "Scheduled plan {PlanId} at {FireAt} (jobId={JobId})",
                plan.Id, fireAt, jobId);
        }

        private async Task CancelScheduledJobAsync(Guid planId)
        {
            try
            {
                // فعلاً فقط پلن Disable می‌شه.
                // برای cancel job، باید ISchedulerService قابلیت QueryByRef رو داشته باشه.
                // placeholder — بعداً می‌سازیمش.
                await Task.CompletedTask;

                _logger.LogDebug(
                    "Cancellation for plan {PlanId} not yet implemented (job remains scheduled but will skip if disabled)",
                    planId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to cancel scheduled job for plan {PlanId}", planId);
            }
        }

        private static DateTimeOffset ComputeFireAt(SchedulePlan plan)
        {
            /* fireAt = Date + AutoLoginAt با offset ایران */
            var localDateTime = plan.Date.ToDateTime(plan.AutoLoginAt);
            return new DateTimeOffset(localDateTime, IranOffset);
        }

        private static SchedulePlanInfoView MapToView(
            SchedulePlan plan,
            Dictionary<Guid, string> accounts,
            Dictionary<string, string> symbols)
        {
            return new SchedulePlanInfoView
            {
                Id = plan.Id,
                Name = plan.Name,
                Date = plan.Date.ToString("yyyy-MM-dd"),
                Enabled = plan.Enabled,
                AutoLoginAt = plan.AutoLoginAt.ToString("HH:mm:ss"),
                AutoRefreshAt = plan.AutoRefreshAt.ToString("HH:mm:ss"),
                Status = plan.Status.ToString().ToLowerInvariant(),
                Message = plan.LastMessage,
                Orders = plan.Orders.Select(o => new ScheduledOrderInfoView
                {
                    Id = o.Id,
                    AccountId = o.AccountId,
                    AccountName = accounts.GetValueOrDefault(o.AccountId),
                    SymbolIsin = o.SymbolIsin,
                    SymbolName = symbols.GetValueOrDefault(o.SymbolIsin),
                    Side = (int)o.Side,
                    Mode = o.Mode == OrderMode.TotalValue ? "totalValue" : "quantity",
                    Quantity = o.Quantity.ToString(),
                    TotalValue = o.TotalValue.ToString(),
                    Time = o.Time.ToString("HH:mm:ss.fff"),
                    Fired = o.Fired,
                }).ToList(),
            };
        }
    }
}