
// Trader.Infrastructure/Configurations/StrategyTuningResultConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public sealed class StrategyTuningResultConfiguration
    : IEntityTypeConfiguration<StrategyTuningResult>
{
    public void Configure(EntityTypeBuilder<StrategyTuningResult> b)
    {
        b.ToTable("StrategyTuningResults");
        b.HasKey(x => x.Id);

        b.Property(x => x.ConfigJson).HasColumnType("nvarchar(max)").IsRequired();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.ErrorMessage).HasMaxLength(2000);
        b.Property(x => x.WindowResultsJson).HasColumnType("nvarchar(max)");

        // Precision 18,4 for all metric columns
        foreach (var p in new[]
        {
            nameof(StrategyTuningResult.AvgReturnArithmeticYearly),
            nameof(StrategyTuningResult.AvgReturnCompoundYearly),
            nameof(StrategyTuningResult.AvgReturnArithmeticSemi),
            nameof(StrategyTuningResult.AvgReturnCompoundSemi),
            nameof(StrategyTuningResult.AvgReturnArithmeticQuarter),
            nameof(StrategyTuningResult.AvgReturnCompoundQuarter),
            nameof(StrategyTuningResult.AvgReturnArithmeticMonthly),
            nameof(StrategyTuningResult.AvgReturnCompoundMonthly),
        })
        {
            b.Property<decimal?>(p).HasPrecision(18, 4);
        }

        b.HasIndex(x => new { x.TuningRunId, x.AvgReturnCompoundYearly });
        b.HasIndex(x => new { x.TuningRunId, x.IsTop10InYearly, x.IsTop10InSemi, x.IsTop10InQuarterly, x.IsTop10InMonthly });
        b.HasIndex(x => x.Generation);
    }
}