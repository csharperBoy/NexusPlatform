using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Domain.Entities;
using Scheduler.Infrastructure.Configurations;

namespace Scheduler.Infrastructure.Data
{
    public class SchedulerDbContext : Base_DbContext
    {
        public SchedulerDbContext(
            DbContextOptions<SchedulerDbContext> options,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }
        public SchedulerDbContext(DbContextOptions<SchedulerDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider()) 
        {
        }

        public virtual DbSet<ScheduledJob> ScheduledJobs { get; set; }
       
        public override void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken))
        {
            //EnsureTrigger("HR.Infrastructure.SqlScript", "CreateAssignmentTrigger.sql", "trg_Assignments_CheckOverlap");
        }
        public override void EnsureViews(CancellationToken cancellationToken = default)
        {
            //EnsureView("Contact.Infrastructure.SqlScript", "CreatePhoneBookInfoViewScript.sql", "PhoneBook_Info_View", "scheduler");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.HasDefaultSchema("scheduler");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("scheduler"));
            modelBuilder.ApplyConfiguration(new ScheduledJobConfiguration());

        }
    }

}
