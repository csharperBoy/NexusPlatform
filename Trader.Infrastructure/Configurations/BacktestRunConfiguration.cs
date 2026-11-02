using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class BacktestRunConfiguration : BaseConfiguration<BacktestRun>
{
    public override void Configure(EntityTypeBuilder<BacktestRun> builder)
    {
        base.Configure(builder);
        builder.ToTable("BacktestRuns", "trader");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.SourceRunId).IsRequired(false);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.StrategyKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StrategyConfigJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.PrimaryTimeFrame).IsRequired();
        builder.Property(x => x.TargetCurrency).HasMaxLength(10).IsRequired(false);

        builder.Property(x => x.CapitalAllocationMode).IsRequired();
        builder.Property(x => x.FixedCapitalAmount).HasPrecision(18, 4).IsRequired(false);
        builder.Property(x => x.StrengthMultiplier).HasPrecision(4, 2).IsRequired();
        builder.Property(x => x.MinDeltaPercent).HasPrecision(5, 4).IsRequired();
        builder.Property(x => x.StopLossPercentage).HasPrecision(9, 4).IsRequired(false);
        builder.Property(x => x.StopLossPrice).HasPrecision(18, 4).IsRequired(false);
        builder.Property(x => x.TakeProfitPercentage).HasPrecision(9, 4).IsRequired(false);
        builder.Property(x => x.TakeProfitPrice).HasPrecision(18, 4).IsRequired(false);
        builder.Property(x => x.DefaultOrderMode).IsRequired();
        builder.Property(x => x.OnMarketClose).IsRequired();
        builder.Property(x => x.OnStop).IsRequired();

        builder.Property(x => x.ConfigSnapshotJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.Property(x => x.DateFrom).IsRequired();
        builder.Property(x => x.DateTo).IsRequired();
        builder.Property(x => x.InitialCapital).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.SlippagePercent).HasPrecision(5, 4).IsRequired();
        builder.Property(x => x.SubCandleTimeFrame).IsRequired(false);
        builder.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(100);

        builder.Property(x => x.FinalEquity).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalReturnPct).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.MaxDrawdownPct).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.SharpeRatio).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.WinRate).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.TotalTrades).IsRequired();
        builder.Property(x => x.WinningTrades).IsRequired();
        builder.Property(x => x.LosingTrades).IsRequired();
        builder.Property(x => x.ProfitFactor).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.AvgWin).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.AvgLoss).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalCommission).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.TotalSlippage).HasPrecision(18, 4).IsRequired();

        builder.Property(x => x.StartedAt).IsRequired(false);
        builder.Property(x => x.CompletedAt).IsRequired(false);
        builder.Property(x => x.DurationMs).IsRequired(false);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000).IsRequired(false);

        builder.HasMany(x => x.Symbols)
            .WithOne()
            .HasForeignKey(s => s.BacktestRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(BacktestRun.Symbols))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_BacktestRun_Status");
        builder.HasIndex(x => x.SourceRunId)
            .HasDatabaseName("IX_BacktestRun_SourceRun");
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_BacktestRun_CreatedAt");
    }
}