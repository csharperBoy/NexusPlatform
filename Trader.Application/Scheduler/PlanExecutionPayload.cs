using Scheduler.Application.Abstractions;
using Scheduler.Application.Attributes;

namespace Trader.Application.Scheduler
{
    [ScheduledJob("trader.plan-execution")]
    public record PlanExecutionPayload(Guid PlanId) : IScheduledJobPayload;
}