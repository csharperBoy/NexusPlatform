using global::Scheduler.Application.Abstractions;
using global::Scheduler.Infrastructure.Data;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.Enums;
using System.Text.Json;


namespace Scheduler.Infrastructure.Jobs
{
   
    /// <summary>
    /// نقطه‌ی ورود همه‌ی jobهای Hangfire.
    /// payload رو deserialize می‌کنه، PreciseDelay می‌زنه، handler رو صدا می‌زنه.
    /// </summary>
    public class HangfireJobDispatcher
    {
        private readonly IServiceProvider _rootSp;
        private readonly JobTypeRegistry _registry;
        private readonly ILogger<HangfireJobDispatcher> _logger;

        public HangfireJobDispatcher(
            IServiceProvider rootSp,
            JobTypeRegistry registry,
            ILogger<HangfireJobDispatcher> logger)
        {
            _rootSp = rootSp;
            _registry = registry;
            _logger = logger;
        }

        [Queue("scheduler")]
        [AutomaticRetry(Attempts = 0)]
        public async Task DispatchAsync(
            Guid scheduledJobId,
            string jobTypeKey,
            string payloadJson,
            long fireAtUnixMs,
            CancellationToken ct)
        {
            using var scope = _rootSp.CreateScope();
            var sp = scope.ServiceProvider;
            var db = sp.GetRequiredService<SchedulerDbContext>();
            var job = await db.ScheduledJobs.FindAsync([scheduledJobId], ct);

            try
            {
                job?.MarkExecuting();
                if (job is not null) await db.SaveChangesAsync(ct);

                var payloadType = _registry.Resolve(jobTypeKey)
                    ?? throw new InvalidOperationException(
                        $"No payload type registered for key '{jobTypeKey}'");

                var payload = JsonSerializer.Deserialize(payloadJson, payloadType)
                    ?? throw new InvalidOperationException("Payload deserialization returned null");

                // ⏱️ انتظار دقیق تا لحظه‌ی هدف
                await PreciseDelay.UntilAsync(fireAtUnixMs, ct);

                // 🎯 resolve handler
                var handlerType = typeof(IScheduledJobHandler<>).MakeGenericType(payloadType);
                var handler = sp.GetService(handlerType)
                    ?? throw new InvalidOperationException(
                        $"No handler registered for payload type {payloadType.FullName}");

                var method = handlerType.GetMethod(nameof(IScheduledJobHandler<IScheduledJobPayload>.ExecuteAsync))!;
                var task = (Task)method.Invoke(handler, [payload, ct])!;
                await task;

                job?.MarkSucceeded();
                if (job is not null) await db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Scheduled job {JobId} ({JobType}) executed successfully",
                    scheduledJobId, jobTypeKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Scheduled job {JobId} ({JobType}) failed",
                    scheduledJobId, jobTypeKey);

                job?.MarkFailed(ex.Message);
                if (job is not null) await db.SaveChangesAsync(CancellationToken.None);

                throw; // Hangfire لاگ می‌کنه
            }
        }
    }
}
