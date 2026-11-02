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
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsinsJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.TimeFramesJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.DateFrom).IsRequired();
        builder.Property(x => x.DateTo).IsRequired();

        builder.Property(x => x.TotalCombinations).IsRequired();
        builder.Property(x => x.CompletedCombinations).IsRequired();
        builder.Property(x => x.TotalCandlesCollected).IsRequired();
        builder.Property(x => x.ProgressPercent).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.CurrentIsin).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.CurrentTimeFrame).IsRequired(false);

        builder.Property(x => x.StartedAt).IsRequired(false);
        builder.Property(x => x.CompletedAt).IsRequired(false);
        builder.Property(x => x.DurationMs).IsRequired(false);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000).IsRequired(false);
        builder.Property(x => x.LogMessagesJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_HistoricalDataCollectionJob_Status");
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_HistoricalDataCollectionJob_CreatedAt");
    }
}