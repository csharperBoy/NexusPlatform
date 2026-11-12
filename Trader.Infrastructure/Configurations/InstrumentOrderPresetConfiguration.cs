using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Infrastructure.Configurations;

public class InstrumentOrderPresetConfiguration : BaseConfiguration<InstrumentOrderPreset>
{
    public override void Configure(EntityTypeBuilder<InstrumentOrderPreset> builder)
    {
        base.Configure(builder);
        builder.ToTable("InstrumentOrderPresets", "trader");

        builder.Property(x => x.InstrumentId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.DefaultSide).HasConversion<int>().IsRequired();
        builder.Property(x => x.DefaultOrderMode).HasConversion<int>().IsRequired();
        builder.Property(x => x.ValidityType).IsRequired();
        builder.Property(x => x.Commission).HasPrecision(18, 4);

       

        builder.Property(x => x.FixedPrice)
            .HasConversion(
                p => p.HasValue ? p.Value.Value : (decimal?)null,
                v => v.HasValue ? new Trader.Domain.ValueObjects.Price(v.Value) : (Trader.Domain.ValueObjects.Price?)null)
            .HasPrecision(18, 4)
            .IsRequired(false);

        builder.Property(x => x.PriceStrategy)
     .HasConversion<int>()
     .IsRequired()
     .HasDefaultValue(OrderPriceStrategy.Fixed);

        builder.Property(x => x.QuantityStrategy)
            .HasConversion<int>()
            .IsRequired()
            .HasDefaultValue(OrderQuantityStrategy.Fixed);

        builder.Property(x => x.FixedQuantity)
            .HasConversion(
                q => q.HasValue ? q.Value.Value : (long?)null,
                v => v.HasValue ? new Trader.Domain.ValueObjects.Quantity(v.Value) : (Trader.Domain.ValueObjects.Quantity?)null)
            .IsRequired(false);

        builder.HasIndex(x => x.InstrumentId)
            .HasDatabaseName("IX_InstrumentOrderPreset_InstrumentId");

        builder.HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}