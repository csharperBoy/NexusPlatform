using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class HistoricalDataCollectionJobConfiguration
    : BaseConfiguration<HistoricalDataCollectionJob>
{
    public override void Configure(EntityTypeBuilder<HistoricalDataCollectionJob> builder)
    {
        base.Configure(builder);
        builder.ToTable("HistoricalDataCollectionJobs", "trader");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Mode).HasConversion<int>().IsRequired();
        builder.Property(x => x.State).HasConversion<int>().IsRequired();

        builder.Property(x => x.IsinsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.TimeFramesJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.DateFrom).IsRequired();
        builder.Property(x => x.DateTo).IsRequired(false);
        builder.Property(x => x.RecurrencePatternJson)
            .HasColumnType("nvarchar(max)").IsRequired(false);

        builder.Property(x => x.SchedulerRecurringId).IsRequired(false);
        builder.Property(x => x.SchedulerJobId).IsRequired(false);

        builder.Property(x => x.TotalCombinations).IsRequired();
        builder.Property(x => x.CompletedCombinations).IsRequired();
        builder.Property(x => x.TotalCandlesCollected).IsRequired();
        builder.Property(x => x.BackfillProgressPercent)
            .HasPrecision(5, 2).IsRequired(false);
        builder.Property(x => x.CurrentIsin).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.CurrentTimeFrame).IsRequired(false);

        builder.Property(x => x.StartedAt).IsRequired(false);
        builder.Property(x => x.StoppedAt).IsRequired(false);
        builder.Property(x => x.LastRunAt).IsRequired(false);
        builder.Property(x => x.LastRunInserted).IsRequired();
        builder.Property(x => x.LastRunSkipped).IsRequired();

        builder.Property(x => x.LastError).HasMaxLength(2000).IsRequired(false);
        builder.Property(x => x.CompletedAt).IsRequired(false);
        builder.Property(x => x.DurationMs).IsRequired(false);
        builder.Property(x => x.LogMessagesJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.State)
            .HasDatabaseName("IX_HistoricalDataCollectionJob_State");
        builder.HasIndex(x => x.Mode)
            .HasDatabaseName("IX_HistoricalDataCollectionJob_Mode");
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_HistoricalDataCollectionJob_CreatedAt");
        builder.HasIndex(x => x.SchedulerRecurringId)
            .HasDatabaseName("IX_HistoricalDataCollectionJob_SchedulerRecurringId")
            .HasFilter("[SchedulerRecurringId] IS NOT NULL");
        builder.HasIndex(x => x.SchedulerJobId)
            .HasDatabaseName("IX_HistoricalDataCollectionJob_SchedulerJobId")
            .HasFilter("[SchedulerJobId] IS NOT NULL");
    }
}