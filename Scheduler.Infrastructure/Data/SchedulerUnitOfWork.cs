using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.Entities;
using Scheduler.Infrastructure.Data;

namespace Scheduler.Infrastructure.Data;

public class SchedulerUnitOfWork
    : EfUnitOfWork<SchedulerDbContext>, ISchedulerUnitOfWork<SchedulerDbContext>
{
    public SchedulerUnitOfWork(
        IRepository<SchedulerDbContext, ScheduledJob, Guid> scheduledJobs,
        IRepository<SchedulerDbContext, RecurringSchedule, Guid> recurringSchedules,
        SchedulerDbContext dbContext,
        IOutboxService<SchedulerDbContext> outboxService,
        ILogger<EfUnitOfWork<SchedulerDbContext>> logger)
        : base(dbContext, outboxService, logger)
    {
        ScheduledJobs = scheduledJobs;
        RecurringSchedules = recurringSchedules;
    }

    public IRepository<SchedulerDbContext, ScheduledJob, Guid> ScheduledJobs { get; }
    public IRepository<SchedulerDbContext, RecurringSchedule, Guid> RecurringSchedules { get; }
}