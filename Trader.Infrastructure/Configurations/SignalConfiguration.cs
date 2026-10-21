using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class SignalConfiguration : BaseConfiguration<Signal>
{
    public override void Configure(EntityTypeBuilder<Signal> builder)
    {
        base.Configure(builder);
        builder.ToTable("Signals", "trader");

        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Action).HasConversion<int>().IsRequired();
        builder.Property(x => x.StrategyKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SourceEvaluationId).IsRequired(false);
        builder.HasIndex(x => x.SourceEvaluationId)
            .HasDatabaseName("IX_Signal_SourceEvaluationId");
        builder.Property(x => x.Strength)
            .HasConversion(
                s => s.Value,
                v => new Trader.Domain.ValueObjects.SignalStrength(v))
            .HasPrecision(9, 4);

        builder.Property(x => x.ProducedAt).IsRequired();
        builder.Property(x => x.ConsumedAt).IsRequired(false);
        builder.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)").IsRequired(false);

        builder.HasIndex(x => new { x.RunId, x.ProducedAt })
            .HasDatabaseName("IX_Signal_Run_ProducedAt");
        builder.HasIndex(x => new { x.RunId, x.Isin })
            .HasDatabaseName("IX_Signal_Run_Isin");
    }
}