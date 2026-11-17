// Trader.Infrastructure/Configurations/StrategyTuningRunConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public sealed class StrategyTuningRunConfiguration
    : IEntityTypeConfiguration<StrategyTuningRun>
{
    public void Configure(EntityTypeBuilder<StrategyTuningRun> b)
    {
        b.ToTable("StrategyTuningRuns");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.StrategyKey).HasMaxLength(64).IsRequired();
        b.Property(x => x.Isin).HasMaxLength(32).IsRequired();
        b.Property(x => x.TargetCurrency).HasMaxLength(8);
        b.Property(x => x.InitialCapital).HasPrecision(18, 2);
        b.Property(x => x.SlippagePercent).HasPrecision(6, 4);
        b.Property(x => x.MutationRate).HasPrecision(5, 4);
        b.Property(x => x.CrossoverRate).HasPrecision(5, 4);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.SelectedScales).HasConversion<int>();
        b.Property(x => x.PrimaryScale).HasConversion<int>();
        b.Property(x => x.Algorithm).HasConversion<int>();
        b.Property(x => x.ErrorMessage).HasMaxLength(2000);
        b.Property(x => x.CreatedBy).HasMaxLength(128);
        b.Property(x => x.ModifiedBy).HasMaxLength(128);

        b.HasIndex(x => new { x.Isin, x.StrategyKey, x.Status });
        b.HasIndex(x => x.CreatedAt);
    }
}
