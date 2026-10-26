using Core.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Scheduler.Domain.Entities;

namespace Scheduler.Application.Abstractions;

public interface ISchedulerUnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    IRepository<TDbContext, ScheduledJob, Guid> ScheduledJobs { get; }
    IRepository<TDbContext, RecurringSchedule, Guid> RecurringSchedules { get; }
}