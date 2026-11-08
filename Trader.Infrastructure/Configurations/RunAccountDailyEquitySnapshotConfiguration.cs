using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class RunAccountDailyEquitySnapshotConfiguration
    : BaseConfiguration<RunAccountDailyEquitySnapshot>
{
    public override void Configure(EntityTypeBuilder<RunAccountDailyEquitySnapshot> builder)
    {
        base.Configure(builder);
        builder.ToTable("RunAccountDailyEquitySnapshots", "trader");

        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.AccountId).IsRequired();
        builder.Property(x => x.TradingDate).IsRequired();

        builder.Property(x => x.OpeningCash).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.OpeningPositionValue).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.OpeningEquity).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.CapturedAt).IsRequired();

        builder.HasIndex(x => new { x.RunId, x.AccountId, x.TradingDate })
            .IsUnique()
            .HasDatabaseName("UX_RunAccountDailyEquity_Run_Account_Date");

        builder.HasIndex(x => new { x.AccountId, x.TradingDate })
            .HasDatabaseName("IX_RunAccountDailyEquity_Account_Date");
    }
}