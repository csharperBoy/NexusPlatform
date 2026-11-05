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
            // ─── Parse Level upfront ───
            ExecutionLogLevel? levelEnum = null;
            if (!string.IsNullOrWhiteSpace(level)
                && !level.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<ExecutionLogLevel>(level, true, out var parsed))
                    levelEnum = parsed;
            }

            // ─── Apply filters + ordering in SQL ───
            // توجه: skip/take در حافظه اعمال می‌شوند (نه در SQL) چون
            // queryOptions یک IQueryable برمی‌گرداند که فقط یک بار
            // Materialize می‌شود. برای Page-fetch در SQL، باید از
            // ApplyPaging استفاده کنیم که نیاز به گسترش interface دارد.
            // در Phase 1، تعداد Logهای هر Run در حد چند هزار است —
            // Memory Skip/Take قابل قبول است. در Phase B اگر لازم شد،
            // GetLogsPagedAsync به IExecutionLogQueryService اضافه می‌شود.
            var filtered = await _logRepo.GetAllAsync(q =>
            {
                if (planId.HasValue)
                    q = q.Where(l => l.PlanId == planId.Value);

                if (levelEnum.HasValue)
                    q = q.Where(l => l.Level == levelEnum.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim();
                    q = q.Where(l => l.Message.Contains(s));
                }

                return q.OrderByDescending(l => l.Timestamp);
            });

            var queryable = filtered.AsQueryable();
            var total = queryable.Count();

            var pageRows = queryable
                .Skip(skip)
                .Take(take)
                .ToList();

            // ─── Plan name lookup — فقط برای Planهای این صفحه ───
            var planIds = pageRows
                .Select(l => l.PlanId)
                .Distinct()
                .ToList();

            var planNames = new Dictionary<Guid, string>();
            if (planIds.Count > 0)
            {
                var plans = await _planRepo.GetAllAsync(
                    q => q.Where(p => planIds.Contains(p.Id)));

                planNames = plans.ToDictionary(p => p.Id, p => p.Name);
            }

            var items = pageRows
                .Select(l => new ExecutionLogInfoView
                {
                    Id = l.Id,
                    PlanId = l.PlanId,
                    PlanName = planNames.GetValueOrDefault(l.PlanId),
                    OrderId = l.OrderId,
                    SymbolIsin = null,
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