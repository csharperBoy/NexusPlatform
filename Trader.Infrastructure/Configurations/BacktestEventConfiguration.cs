using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class BacktestEventConfiguration : BaseConfiguration<BacktestEvent>
{
    public override void Configure(EntityTypeBuilder<BacktestEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable("BacktestEvents", "trader");

        builder.Property(x => x.BacktestRunId).IsRequired();
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Severity).HasConversion<int>().IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.Message).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)").IsRequired(false);
        builder.Property(x => x.Isin).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.CandleTime).IsRequired(false);
        builder.Property(x => x.TradeId).IsRequired(false);

        builder.HasIndex(x => new { x.BacktestRunId, x.OccurredAt })
            .HasDatabaseName("IX_BacktestEvent_Run_Time");

        builder.HasIndex(x => new { x.BacktestRunId, x.Type })
            .HasDatabaseName("IX_BacktestEvent_Run_Type");

        builder.HasIndex(x => new { x.BacktestRunId, x.Severity })
            .HasDatabaseName("IX_BacktestEvent_Run_Severity");
    }
}