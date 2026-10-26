using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduler.Domain.Entities;

namespace Scheduler.Infrastructure.Configurations;

public class RecurringScheduleConfiguration : BaseConfiguration<RecurringSchedule>
{
    public override void Configure(EntityTypeBuilder<RecurringSchedule> builder)
    {
        base.Configure(builder);
        builder.ToTable("RecurringSchedules", "scheduler");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("UX_RecurringSchedule_Name");

        builder.Property(x => x.JobType).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PayloadJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.RecurrencePatternJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(100);

        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.EndAt).IsRequired(false);
        builder.Property(x => x.MaxOccurrences).IsRequired(false);
        builder.Property(x => x.MaxFailures).IsRequired().HasDefaultValue(10);

        builder.Property(x => x.State).HasConversion<int>().IsRequired();
        builder.Property(x => x.OccurredCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.FailureCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.LastRunAt).IsRequired(false);
        builder.Property(x => x.NextRunAt).IsRequired(false);
        builder.Property(x => x.CurrentScheduledJobId).IsRequired(false);

        builder.HasIndex(x => x.State)
            .HasDatabaseName("IX_RecurringSchedule_State");

        builder.HasIndex(x => new { x.State, x.NextRunAt })
            .HasDatabaseName("IX_RecurringSchedule_State_NextRun");
    }
}