using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class BacktestSymbolConfiguration : BaseConfiguration<BacktestSymbol>
{
    public override void Configure(EntityTypeBuilder<BacktestSymbol> builder)
    {
        base.Configure(builder);
        builder.ToTable("BacktestSymbols", "trader");

        builder.Property(x => x.BacktestRunId).IsRequired();
        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.AllocationPercentage).HasPrecision(9, 4).IsRequired();

        builder.HasIndex(x => new { x.BacktestRunId, x.Isin })
            .IsUnique()
            .HasDatabaseName("UX_BacktestSymbol_Run_Isin");
    }
}