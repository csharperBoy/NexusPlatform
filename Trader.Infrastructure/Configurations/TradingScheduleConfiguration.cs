using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class TradingScheduleConfiguration : BaseConfiguration<TradingSchedule>
{
    public override void Configure(EntityTypeBuilder<TradingSchedule> builder)
    {
        base.Configure(builder);
        builder.ToTable("TradingSchedules", "trader");

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("UX_TradingSchedule_Code");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);

        // TimeOnly → SQL Server time
        builder.Property(x => x.PreMarketStart)
            .HasColumnType("time(0)")
            .IsRequired();
        builder.Property(x => x.SessionStart)
            .HasColumnType("time(0)")
            .IsRequired();
        builder.Property(x => x.SessionEnd)
            .HasColumnType("time(0)")
            .IsRequired();
        builder.Property(x => x.AfternoonStart)
            .HasColumnType("time(0)")
            .IsRequired(false);
        builder.Property(x => x.AfternoonEnd)
            .HasColumnType("time(0)")
            .IsRequired(false);
        builder.Property(x => x.AfterHoursEnd)
            .HasColumnType("time(0)")
            .IsRequired(false);

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_TradingSchedule_IsActive");
    }
}