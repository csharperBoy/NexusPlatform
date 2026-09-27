using Core.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Services
{
    public class ExecutionLogService : IExecutionLogQueryService
    {
        private readonly IRepository<TraderDbContext, ExecutionLog, Guid> _logRepo;
        private readonly IRepository<TraderDbContext, SchedulePlan, Guid> _planRepo;

        public ExecutionLogService(
            IRepository<TraderDbContext, ExecutionLog, Guid> logRepo,
            IRepository<TraderDbContext, SchedulePlan, Guid> planRepo)
        {
            _logRepo = logRepo;
            _planRepo = planRepo;
        }

        public async Task<ExecutionLogQueryResult> GetLogsAsync(
            Guid? planId,
            string? level,
            string? search,
            int skip,
            int take,
            CancellationToken ct = default)
        {
            /* ─── همه‌ی planها برای lookup ─── */
            var plans = (await _planRepo.GetAllAsync())
                .ToDictionary(p => p.Id, p => p.Name);

            /* ─── همه‌ی logها رو می‌گیریم ─── */
            /* برای داده‌ی زیاد، باید ایندکس و صفحه‌بندی سمت DB باشه */
            var query = await _logRepo.GetAllAsync(
                queryOptions: q => q.OrderByDescending(l => l.Timestamp));

            var filtered = query.AsEnumerable();

            if (planId.HasValue)
                filtered = filtered.Where(l => l.PlanId == planId.Value);

            if (!string.IsNullOrWhiteSpace(level) &&
                !level.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<ExecutionLogLevel>(level, true, out var levelEnum))
                    filtered = filtered.Where(l => l.Level == levelEnum);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLowerInvariant();
                filtered = filtered.Where(l =>
                    l.Message.ToLowerInvariant().Contains(q));
            }

            var total = filtered.Count();

            var items = filtered
                .Skip(skip)
                .Take(take)
                .Select(l => new ExecutionLogInfoView
                {
                    Id = l.Id,
                    PlanId = l.PlanId,
                    PlanName = plans.GetValueOrDefault(l.PlanId),
                    OrderId = l.OrderId,
                    SymbolIsin = null,  // فعلاً null — بعداً می‌تونه از order بیاد
                    Timestamp = l.Timestamp.ToUnixTimeMilliseconds(),
                    Level = l.Level.ToString().ToLowerInvariant(),
                    Message = l.Message,
                    Code = l.Code,
                })
                .ToList();

            return new ExecutionLogQueryResult
            {
                Items = items,
                TotalCount = total,
            };
        }
    }
}