using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class RunDailyEquitySnapshotConfiguration : BaseConfiguration<RunDailyEquitySnapshot>
{
    public override void Configure(EntityTypeBuilder<RunDailyEquitySnapshot> builder)
    {
        base.Configure(builder);
        builder.ToTable("RunDailyEquitySnapshots", "trader");

        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.TradingDate).IsRequired();

        builder.Property(x => x.OpeningCash).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.OpeningPositionValue).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.OpeningEquity).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.CapturedAt).IsRequired();

        // یک رکورد per Run per Day
        builder.HasIndex(x => new { x.RunId, x.TradingDate })
            .IsUnique()
            .HasDatabaseName("UX_RunDailyEquitySnapshot_Run_Date");

        // Query: opening برای Run در یک روز
        builder.HasIndex(x => x.TradingDate)
            .HasDatabaseName("IX_RunDailyEquitySnapshot_Date");
    }
}