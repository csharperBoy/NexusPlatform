using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public sealed class CorporateActionConfiguration
    : IEntityTypeConfiguration<CorporateAction>
{
    public void Configure(EntityTypeBuilder<CorporateAction> b)
    {
        b.ToTable("CorporateActions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Isin).HasMaxLength(32).IsRequired();
        b.Property(x => x.ExDateUtc).IsRequired();
        b.Property(x => x.ActionType).HasConversion<int>();

        b.Property(x => x.Amount).HasPrecision(18, 4);
        b.Property(x => x.Ratio).HasPrecision(18, 8);
        b.Property(x => x.SubscriptionPrice).HasPrecision(18, 4);

        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.SourceId).HasMaxLength(64);
        b.Property(x => x.RawText).HasColumnType("nvarchar(max)");

        b.Property(x => x.CreatedBy).HasMaxLength(128);
        b.Property(x => x.ModifiedBy).HasMaxLength(128);

        b.HasIndex(x => new { x.Isin, x.ExDateUtc });
        b.HasIndex(x => new { x.Isin, x.ActionType });
    }
}