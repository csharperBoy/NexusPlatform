using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;
using Trader.Domain.ValueObjects;

namespace Trader.Infrastructure.Configurations;

public class TradingRunConfiguration : BaseConfiguration<TradingRun>
{
    public override void Configure(EntityTypeBuilder<TradingRun> builder)
    {
        base.Configure(builder);
        builder.ToTable("TradingRuns", "trader");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.State).HasConversion<int>().IsRequired();

        builder.Property(x => x.StrategyKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StrategyConfigJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.PrimaryTimeFrame).HasConversion<int>().IsRequired();

        // ─── CapitalAllocation (Owned) ───
        builder.OwnsOne(x => x.CapitalAllocation, cb =>
        {
            cb.Property(p => p.Mode)
                .HasConversion<int>()
                .HasColumnName("CapitalAllocationMode")
                .IsRequired();

            // FixedAmount (Money?) — columns داخل این block
            cb.OwnsOne(p => p.FixedAmount, mb =>
            {
                mb.Property(m => m.Amount)
                    .HasColumnName("FixedCapitalAmount")
                    .HasPrecision(18, 4);
                mb.Property(m => m.Currency)
                    .HasColumnName("FixedCapitalCurrency")
                    .HasMaxLength(10);
            });

            // FixedAmount کل اختیاری است — نه Propertyها
            cb.Navigation(p => p.FixedAmount).IsRequired(false);
        });

        // ─── StopLoss (Owned) ───
        builder.OwnsOne(x => x.StopLoss, sb =>
        {
            sb.Property(p => p.Percentage)
                .HasColumnName("StopLossPercentage")
                .HasPrecision(9, 4);

            sb.Property(p => p.AbsolutePrice)
                .HasConversion(
                    p => p.HasValue ? p.Value.Value : (decimal?)null,
                    v => v.HasValue ? new Price(v.Value) : (Price?)null)
                .HasColumnName("StopLossPrice")
                .HasPrecision(18, 4);
        });
        builder.Navigation(x => x.StopLoss).IsRequired(false);

        // ─── TakeProfit (Owned) ───
        builder.OwnsOne(x => x.TakeProfit, tb =>
        {
            tb.Property(p => p.Percentage)
                .HasColumnName("TakeProfitPercentage")
                .HasPrecision(9, 4);

            tb.Property(p => p.AbsolutePrice)
                .HasConversion(
                    p => p.HasValue ? p.Value.Value : (decimal?)null,
                    v => v.HasValue ? new Price(v.Value) : (Price?)null)
                .HasColumnName("TakeProfitPrice")
                .HasPrecision(18, 4);
        });
        builder.Navigation(x => x.TakeProfit).IsRequired(false);
        builder.Property(x => x.DefaultOrderMode).HasConversion<int>().IsRequired();
        builder.Property(x => x.OnMarketClose).HasConversion<int>().IsRequired();
        builder.Property(x => x.OnStop).HasConversion<int>().IsRequired();

        builder.Property(x => x.StartedAt).IsRequired(false);
        builder.Property(x => x.StoppedAt).IsRequired(false);

        // ─── Relations ───
        builder.HasMany(x => x.Symbols)
            .WithOne()
            .HasForeignKey(s => s.TradingRunId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Accounts)
            .WithOne()
            .HasForeignKey(a => a.TradingRunId)
            .OnDelete(DeleteBehavior.Cascade);

        // Backing field mapping
        builder.Metadata
            .FindNavigation(nameof(TradingRun.Symbols))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata
            .FindNavigation(nameof(TradingRun.Accounts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.State)
            .HasDatabaseName("IX_TradingRun_State");
        builder.HasIndex(x => x.StrategyKey)
            .HasDatabaseName("IX_TradingRun_StrategyKey");
        builder.HasIndex(x => new { x.OwnerPersonId, x.State })
            .HasDatabaseName("IX_TradingRun_Owner_State");
    }
}