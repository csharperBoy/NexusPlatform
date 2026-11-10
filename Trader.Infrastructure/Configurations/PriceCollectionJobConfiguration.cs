using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class PriceCollectionJobConfiguration : BaseConfiguration<PriceCollectionJob>
{
    public override void Configure(EntityTypeBuilder<PriceCollectionJob> builder)
    {
        base.Configure(builder);
        builder.ToTable("PriceCollectionJobs", "trader");

        builder.Property(x => x.PriceSourceId).IsRequired();
        builder.Property(x => x.Mode).HasConversion<int>().IsRequired();
        builder.Property(x => x.State).HasConversion<int>().IsRequired();
        builder.Property(x => x.RecurrencePatternJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.SchedulerRecurringId).IsRequired(false);
        builder.Property(x => x.SchedulerJobId).IsRequired(false);
        builder.Property(x => x.StartedAt).IsRequired(false);
        builder.Property(x => x.StoppedAt).IsRequired(false);
        builder.Property(x => x.LastRunAt).IsRequired(false);
        builder.Property(x => x.LastRunInserted).IsRequired();
        builder.Property(x => x.LastRunSkipped).IsRequired();
        builder.Property(x => x.TotalInserted).IsRequired();
        builder.Property(x => x.BackfillProgressPercent).HasPrecision(5, 2).IsRequired(false);
        builder.Property(x => x.LastError).HasMaxLength(2000).IsRequired(false);

        builder.HasOne<PriceSource>()
            .WithMany()
            .HasForeignKey(x => x.PriceSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PriceSourceId)
            .HasDatabaseName("IX_PriceCollectionJob_Source");
        builder.HasIndex(x => x.State)
            .HasDatabaseName("IX_PriceCollectionJob_State");
        builder.HasIndex(x => x.Mode)
            .HasDatabaseName("IX_PriceCollectionJob_Mode");
    }
}