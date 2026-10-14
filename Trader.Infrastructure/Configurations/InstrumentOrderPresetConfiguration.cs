using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

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

        builder.Property(x => x.DefaultPrice)
            .HasConversion(p => p.Value, v => new Trader.Domain.ValueObjects.Price(v))
            .HasPrecision(18, 4);

        builder.Property(x => x.DefaultQuantity)
            .HasConversion(q => q.Value, v => new Trader.Domain.ValueObjects.Quantity(v));

        builder.HasIndex(x => x.InstrumentId)
            .HasDatabaseName("IX_InstrumentOrderPreset_InstrumentId");

        builder.HasOne<Instrument>()
            .WithMany()
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}