using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Domain.Entities;


namespace Scheduler.Infrastructure.Configurations
{
    public class ScheduledJobConfiguration : BaseConfiguration<ScheduledJob>
    {
        public override void Configure(EntityTypeBuilder<ScheduledJob> builder)
        {
            base.Configure(builder);

            builder.ToTable("ScheduledJobs", "scheduler");



                builder.Property(x => x.JobType).IsRequired().HasMaxLength(200);
            builder.Property(x => x.HangfireJobId).IsRequired().HasMaxLength(100);
            builder.Property(x => x.PayloadJson).IsRequired();
            builder.Property(x => x.Error).HasMaxLength(2000);
            builder.Property(x => x.PreFireBuffer)
                    .HasConversion(
                        v => v.Ticks,
                        v => TimeSpan.FromTicks(v));

            builder.HasIndex(x => x.FireAt);
            builder.HasIndex(x => x.State);
            builder.HasIndex(x => x.HangfireJobId).IsUnique();
         


        }

    }
}
