using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class CurrencyRateConfiguration : BaseConfiguration<CurrencyRate>
{
    public override void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        base.Configure(builder);
        builder.ToTable("CurrencyRates", "trader");

        builder.Property(x => x.FromCurrency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.ToCurrency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Rate)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.EffectiveAt).IsRequired();
        builder.Property(x => x.Source).IsRequired().HasMaxLength(50);

        // ─── Unique: یک نرخ برای یک (Pair, EffectiveAt) ───
        builder.HasIndex(x => new { x.FromCurrency, x.ToCurrency, x.EffectiveAt })
            .IsUnique()
            .HasDatabaseName("UX_CurrencyRate_Pair_EffectiveAt");

        // ─── Query: آخرین نرخ برای یک Pair قبل از یک زمان ───
        builder.HasIndex(x => new { x.FromCurrency, x.ToCurrency, x.EffectiveAt })
            .IsDescending(false, false, true)
            .HasDatabaseName("IX_CurrencyRate_Pair_TimeDesc");
    }
}