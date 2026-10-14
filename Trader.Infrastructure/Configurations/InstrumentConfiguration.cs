using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class InstrumentConfiguration : BaseConfiguration<Instrument>
{
    public override void Configure(EntityTypeBuilder<Instrument> builder)
    {
        base.Configure(builder);
        builder.ToTable("Instruments", "trader");

        builder.Property(x => x.Isin)
            .IsRequired()
            .HasMaxLength(20);
        builder.HasIndex(x => x.Isin)
            .IsUnique()
            .HasDatabaseName("UX_Instrument_Isin");

        builder.Property(x => x.SymbolName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.Market).HasConversion<int>().IsRequired();

        builder.Property(x => x.BoardCode).HasMaxLength(50);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        // OptionContract به عنوان Owned Type
        builder.OwnsOne(x => x.OptionContract, ob =>
        {
            ob.Property(p => p.Type).HasConversion<int>().HasColumnName("OptionType");
            ob.Property(p => p.StrikePrice)
                .HasConversion(
                    p => p.Value,
                    v => new Trader.Domain.ValueObjects.Price(v))
                .HasColumnName("OptionStrikePrice")
                .HasPrecision(18, 4);
            ob.Property(p => p.ExpirationDate).HasColumnName("OptionExpirationDate");
            ob.Property(p => p.UnderlyingIsin)
                .HasColumnName("OptionUnderlyingIsin")
                .HasMaxLength(20);
        });

        builder.HasIndex(x => x.Market)
            .HasDatabaseName("IX_Instrument_Market");
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_Instrument_IsActive");
    }
}