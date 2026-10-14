using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class TradingRunSymbolConfiguration : BaseConfiguration<TradingRunSymbol>
{
    public override void Configure(EntityTypeBuilder<TradingRunSymbol> builder)
    {
        base.Configure(builder);
        builder.ToTable("TradingRunSymbols", "trader");

        builder.Property(x => x.TradingRunId).IsRequired();
        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);

        builder.Property(x => x.Allocation)
            .HasConversion(
                a => a.Percentage,
                v => new Trader.Domain.ValueObjects.SymbolAllocation(v))
            .HasColumnName("AllocationPercentage")
            .HasPrecision(9, 4);

        builder.HasIndex(x => new { x.TradingRunId, x.Isin })
            .IsUnique()
            .HasDatabaseName("UX_TradingRunSymbol_Run_Isin");
    }
}