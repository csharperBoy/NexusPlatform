using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class BacktestTradeConfiguration : BaseConfiguration<BacktestTrade>
{
    public override void Configure(EntityTypeBuilder<BacktestTrade> builder)
    {
        base.Configure(builder);
        builder.ToTable("BacktestTrades", "trader");

        builder.Property(x => x.BacktestRunId).IsRequired();
        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);

        builder.Property(x => x.EntryTime).IsRequired();
        builder.Property(x => x.LastEntryTime).IsRequired();
        builder.Property(x => x.TotalEntryQuantity).IsRequired();
        builder.Property(x => x.WeightedAvgEntryPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalBuyCommission).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.EntryFillsJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.Property(x => x.ExitTime).IsRequired();
        builder.Property(x => x.ExitPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.ExitQuantity).IsRequired();
        builder.Property(x => x.TotalSellCommission).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.ExitReason).HasConversion<int>().IsRequired();

        builder.Property(x => x.GrossPnL).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.NetPnL).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.NetPnLPct).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.HoldingDurationSeconds).IsRequired();

        builder.HasIndex(x => new { x.BacktestRunId, x.ExitTime })
            .HasDatabaseName("IX_BacktestTrade_Run_ExitTime");
        builder.HasIndex(x => new { x.BacktestRunId, x.Isin })
            .HasDatabaseName("IX_BacktestTrade_Run_Isin");
        builder.HasIndex(x => new { x.BacktestRunId, x.NetPnL })
            .HasDatabaseName("IX_BacktestTrade_Run_NetPnL");
    }
}