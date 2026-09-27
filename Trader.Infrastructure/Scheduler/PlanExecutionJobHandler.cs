using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Trader.Application.Scheduler;
using Trader.Application.Scheduler.Services;

namespace Trader.Infrastructure.Scheduler
{
    public class PlanExecutionJobHandler : IScheduledJobHandler<PlanExecutionPayload>
    {
        private readonly IPlanExecutor _executor;
        private readonly ILogger<PlanExecutionJobHandler> _logger;

        public PlanExecutionJobHandler(
            IPlanExecutor executor,
            ILogger<PlanExecutionJobHandler> logger)
        {
            _executor = executor;
            _logger = logger;
        }

        public async Task ExecuteAsync(
            PlanExecutionPayload payload,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "Executing plan {PlanId}", payload.PlanId);

            await _executor.ExecuteAsync(payload.PlanId, ct);
        }
    }
}