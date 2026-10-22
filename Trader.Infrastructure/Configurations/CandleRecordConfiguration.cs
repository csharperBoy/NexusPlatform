using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class CandleRecordConfiguration : BaseConfiguration<CandleRecord>
{
    public override void Configure(EntityTypeBuilder<CandleRecord> builder)
    {
        base.Configure(builder);
        builder.ToTable("Candles", "trader");

        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.TimeFrame).IsRequired();
        builder.Property(x => x.OpenTime).IsRequired();

        builder.Property(x => x.Open).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.High).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Low).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Close).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Volume).IsRequired();

        builder.Property(x => x.Source).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FetchedAt).IsRequired();

        // ─── Unique: یک کندل برای یک (Isin, TF, OpenTime) ───
        builder.HasIndex(x => new { x.Isin, x.TimeFrame, x.OpenTime })
            .IsUnique()
            .HasDatabaseName("UX_Candle_Isin_Tf_Time");

        // ─── Query: آخرین N کندل برای (Isin, TF) ───
        builder.HasIndex(x => new { x.Isin, x.TimeFrame, x.OpenTime })
            .HasDatabaseName("IX_Candle_Isin_Tf_Time_Desc");
    }
}