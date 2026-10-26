using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class TradingEventConfiguration : BaseConfiguration<TradingEvent>
{
    public override void Configure(EntityTypeBuilder<TradingEvent> builder)
    {
        base.Configure(builder);
        builder.ToTable("TradingEvents", "trader");

        builder.Property(x => x.RunId).IsRequired(false);
        builder.Property(x => x.Isin).HasMaxLength(20).IsRequired(false);
        builder.Property(x => x.AccountId).IsRequired(false);
        builder.Property(x => x.SignalId).IsRequired(false);
        builder.Property(x => x.OrderIntentId).IsRequired(false);
        builder.Property(x => x.BrokerOrderId).IsRequired(false);
        builder.Property(x => x.TradeId).IsRequired(false);
        builder.Property(x => x.PositionId).IsRequired(false);

        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Severity).HasConversion<int>().IsRequired();
        builder.Property(x => x.Stage).HasConversion<int>().IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.PayloadJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // ─── Indexes ───

        // Query اصلی: لاگ‌های یک Run در یک بازه
        builder.HasIndex(x => new { x.RunId, x.OccurredAt })
            .HasDatabaseName("IX_TradingEvent_Run_Time");

        // Query: همه‌ی رویدادهای یک نماد (در همه‌ی Runها)
        builder.HasIndex(x => new { x.Isin, x.OccurredAt })
            .HasDatabaseName("IX_TradingEvent_Isin_Time");

        // Query: فیلتر بر اساس Stage و Severity
        builder.HasIndex(x => new { x.Stage, x.Severity, x.OccurredAt })
            .HasDatabaseName("IX_TradingEvent_Stage_Sev_Time");

        // Query: پاک‌سازی (Retention) — DELETE WHERE OccurredAt < cutoff
        builder.HasIndex(x => x.OccurredAt)
            .HasDatabaseName("IX_TradingEvent_Time");

        // Query: Trace یک Signal در کل Pipeline
        builder.HasIndex(x => x.SignalId)
            .HasDatabaseName("IX_TradingEvent_SignalId")
            .HasFilter("[SignalId] IS NOT NULL");

        builder.HasIndex(x => x.OrderIntentId)
            .HasDatabaseName("IX_TradingEvent_OrderIntentId")
            .HasFilter("[OrderIntentId] IS NOT NULL");

        builder.HasIndex(x => x.PositionId)
            .HasDatabaseName("IX_TradingEvent_PositionId")
            .HasFilter("[PositionId] IS NOT NULL");

        // Query: نمایش فقط Error/Critical (Filtered Index — سبک‌تر)
        builder.HasIndex(x => new { x.Severity, x.OccurredAt })
            .HasDatabaseName("IX_TradingEvent_Errors_Time")
            .HasFilter("[Severity] >= 3");
    }
}