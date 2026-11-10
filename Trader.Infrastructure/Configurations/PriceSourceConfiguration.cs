using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class PriceSourceConfiguration : BaseConfiguration<PriceSource>
{
    public override void Configure(EntityTypeBuilder<PriceSource> builder)
    {
        base.Configure(builder);
        builder.ToTable("PriceSources", "trader");

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique()
            .HasDatabaseName("UX_PriceSource_Code");

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        builder.Property(x => x.ConfigJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.FromAsset).IsRequired().HasMaxLength(10);
        builder.Property(x => x.ToAsset).IsRequired().HasMaxLength(10);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => new { x.FromAsset, x.ToAsset })
            .HasDatabaseName("IX_PriceSource_Pair");
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_PriceSource_IsActive");
    }
}