using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class StrategyEvaluationConfiguration : BaseConfiguration<StrategyEvaluation>
{
    public override void Configure(EntityTypeBuilder<StrategyEvaluation> builder)
    {
        base.Configure(builder);
        builder.ToTable("StrategyEvaluations", "trader");

        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.StrategyKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EvaluatedAt).IsRequired();

        builder.Property(x => x.PriceAtEval)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.FeaturesJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Action).HasConversion<int>().IsRequired();
        builder.Property(x => x.Strength).HasPrecision(9, 4).IsRequired();

        builder.Property(x => x.SignalId).IsRequired(false);
        builder.Property(x => x.OutcomeJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.OutcomeUpdatedAt).IsRequired(false);

        builder.HasIndex(x => new { x.RunId, x.Isin, x.EvaluatedAt })
            .HasDatabaseName("IX_StrategyEval_Run_Isin_Time");
        builder.HasIndex(x => new { x.Isin, x.EvaluatedAt })
            .HasDatabaseName("IX_StrategyEval_Isin_Time");
        builder.HasIndex(x => x.SignalId)
            .HasDatabaseName("IX_StrategyEval_SignalId");
        builder.HasIndex(x => x.EvaluatedAt)
            .HasDatabaseName("IX_StrategyEval_EvaluatedAt");

        // Partial Index برای Training Dataset (فقط Buy/Sell)
        builder.HasIndex(x => x.Action)
            .HasFilter("[Action] <> 0")
            .HasDatabaseName("IX_StrategyEval_NonHold");
    }
}