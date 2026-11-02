using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class BacktestDailyEquityConfiguration : BaseConfiguration<BacktestDailyEquity>
{
    public override void Configure(EntityTypeBuilder<BacktestDailyEquity> builder)
    {
        base.Configure(builder);
        builder.ToTable("BacktestDailyEquities", "trader");

        builder.Property(x => x.BacktestRunId).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Equity).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Cash).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.PositionValue).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.DailyReturnPct).HasPrecision(9, 4).IsRequired();

        builder.HasIndex(x => new { x.BacktestRunId, x.Date })
            .IsUnique()
            .HasDatabaseName("UX_BacktestDailyEquity_Run_Date");
    }
}