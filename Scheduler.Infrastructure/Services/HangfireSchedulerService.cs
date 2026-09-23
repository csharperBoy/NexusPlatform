using global::Scheduler.Application.Abstractions;
using global::Scheduler.Application.Models;
using global::Scheduler.Application.Queries;
using global::Scheduler.Domain.Entities;
using global::Scheduler.Infrastructure.Data;
using global::Scheduler.Infrastructure.Jobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;
using Scheduler.Domain.Entities;
using Scheduler.Infrastructure.Jobs;
using System.Reflection;
using System.Text.Json;

namespace Scheduler.Infrastructure.Services
{
    
 class HangfireSchedulerService : ISchedulerService
    {
        private readonly IBackgroundJobClient _jobs;
        private readonly JobTypeRegistry _registry;
        private readonly SchedulerDbContext _db;
        private readonly SchedulerOptions _options;
        private readonly ILogger<HangfireSchedulerService> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public HangfireSchedulerService(
            IBackgroundJobClient jobs,
            JobTypeRegistry registry,
            SchedulerDbContext db,
            IOptions<SchedulerOptions> options,
            ILogger<HangfireSchedulerService> logger)
        {
            _jobs = jobs;
            _registry = registry;
            _db = db;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<Guid> ScheduleAsync<TPayload>(
            TPayload payload,
            DateTimeOffset fireAt,
            string? queue = null,
            TimeSpan? preFireBuffer = null,
            CancellationToken ct = default)
            where TPayload : IScheduledJobPayload
        {
            var jobTypeKey = _registry.GetKey(typeof(TPayload));
            var preFire = preFireBuffer ?? _options.DefaultPreFireBuffer;
            var triggerAt = fireAt - preFire;

            if (triggerAt <= DateTimeOffset.UtcNow)
                triggerAt = DateTimeOffset.UtcNow.AddSeconds(1);

            var payloadJson = JsonSerializer.Serialize(payload, JsonOpts);

            // ۱) رکورد دامنه‌ای
            var domainJob = ScheduledJob.Create(
                jobType: jobTypeKey,
                hangfireJobId: "PENDING",       // بعد از schedule آپدیت میشه
                payloadJson: payloadJson,
                fireAt: fireAt,
                preFireBuffer: preFire);

            _db.ScheduledJobs.Add(domainJob);
            await _db.SaveChangesAsync(ct);

            // ۲) زمان‌بندی در Hangfire
            var hangfireJobId = _jobs.Schedule<HangfireJobDispatcher>(
                d => d.DispatchAsync(
                    domainJob.Id,
                    jobTypeKey,
                    payloadJson,
                    fireAt.ToUnixTimeMilliseconds(),
                    CancellationToken.None),
                triggerAt);

            // ۳) آپدیت hangfireJobId
            _db.Entry(domainJob).Property(nameof(ScheduledJob.HangfireJobId)).CurrentValue = hangfireJobId;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Scheduled job {JobId} type={Type} fireAt={FireAt} triggerAt={TriggerAt}",
                domainJob.Id, jobTypeKey, fireAt, triggerAt);

            return domainJob.Id;
        }

        public async Task<bool> CancelAsync(Guid jobId, CancellationToken ct = default)
        {
            var job = await _db.ScheduledJobs.FindAsync([jobId], ct);
            if (job is null || job.State != Domain.Enums.ScheduledJobState.Pending)
                return false;

            var cancelled = _jobs.Delete(job.HangfireJobId);
            if (cancelled)
            {
                job.MarkCancelled();
                await _db.SaveChangesAsync(ct);
            }
            return cancelled;
        }

        public async Task<ScheduledJobInfo?> GetAsync(Guid jobId, CancellationToken ct = default)
        {
            var job = await _db.ScheduledJobs.FindAsync([jobId], ct);
            return job is null ? null : ToInfo(job);
        }

        public async Task<IReadOnlyList<ScheduledJobInfo>> QueryAsync(
            SchedulerQuery query, CancellationToken ct = default)
        {
            var q = _db.ScheduledJobs.AsQueryable();

            if (query.JobType is not null)
                q = q.Where(x => x.JobType == query.JobType);            
            if (query.State is not null)
                q = q.Where(x => x.State == query.State);
            if (query.FireAtFrom is not null)
                q = q.Where(x => x.FireAt >= query.FireAtFrom);
            if (query.FireAtTo is not null)
                q = q.Where(x => x.FireAt <= query.FireAtTo);

            return await q
                .OrderBy(x => x.FireAt)
                .Skip(query.Skip)
                .Take(query.Take)
                .Select(x => ToInfo(x))
                .ToListAsync(ct);
        }

        private static ScheduledJobInfo ToInfo(ScheduledJob j) => new(
            j.Id,  j.JobType, j.State,
            j.FireAt, j.StartedAt, j.CompletedAt, j.Error);

       
    }
}
