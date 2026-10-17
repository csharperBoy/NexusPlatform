using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;
using Trader.Domain.ValueObjects;

namespace Trader.Infrastructure.Configurations;

public class PositionConfiguration : BaseConfiguration<Position>
{
    public override void Configure(EntityTypeBuilder<Position> builder)
    {
        base.Configure(builder);
        builder.ToTable("Positions", "trader");

        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.TraderAccountId).IsRequired();
        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.State).HasConversion<int>().IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(q => q.Value, v => new Quantity(v))
            .IsRequired();

        builder.Property(x => x.AverageEntryPrice)
            .HasConversion(p => p.Value, v => new Price(v))
            .HasPrecision(18, 4)
            .IsRequired();

        // ─── RealizedPnL (Money — class) ───
        builder.OwnsOne(x => x.UnrealizedPnL, ub =>
        {
            ub.Property(m => m.Amount)
                .HasColumnName("UnrealizedPnLAmount")
                .HasPrecision(18, 4)
                .IsRequired(false);
            ub.Property(m => m.Currency)
                .HasColumnName("UnrealizedPnLCurrency")
                .HasMaxLength(10)
                .IsRequired(false);
        });
        builder.Navigation(x => x.UnrealizedPnL).IsRequired(false);

        // ─── UnrealizedPnL (Money? — nullable reference) ───
        builder.OwnsOne(x => x.UnrealizedPnL, ub =>
        {
            ub.Property(m => m.Amount)
                .HasColumnName("UnrealizedPnLAmount")
                .HasPrecision(18, 4);
            ub.Property(m => m.Currency)
                .HasColumnName("UnrealizedPnLCurrency")
                .HasMaxLength(10);
        });
        builder.Navigation(x => x.UnrealizedPnL).IsRequired(false);

        // ─── SL/TP Prices (Price? struct) ───
        builder.Property(x => x.StopLossPrice)
            .HasConversion(
                p => p.HasValue ? p.Value.Value : (decimal?)null,
                v => v.HasValue ? new Price(v.Value) : (Price?)null)
            .HasPrecision(18, 4);

        builder.Property(x => x.TakeProfitPrice)
            .HasConversion(
                p => p.HasValue ? p.Value.Value : (decimal?)null,
                v => v.HasValue ? new Price(v.Value) : (Price?)null)
            .HasPrecision(18, 4);

        builder.Property(x => x.OpenedAt).IsRequired();
        builder.Property(x => x.ClosedAt).IsRequired(false);

        builder.HasIndex(x => new { x.RunId, x.Isin, x.State })
            .HasDatabaseName("IX_Position_Run_Isin_State");
        builder.HasIndex(x => new { x.TraderAccountId, x.Isin })
            .HasDatabaseName("IX_Position_Account_Isin");
    }
}