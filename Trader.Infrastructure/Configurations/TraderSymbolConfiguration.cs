using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations
{
    public class TraderSymbolConfiguration : BaseConfiguration<TraderSymbol>
    {
        public override void Configure(EntityTypeBuilder<TraderSymbol> builder)
        {
            base.Configure(builder);
            builder.ToTable("Symbols", "trader");

            builder.Property(x => x.SymbolName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.SymbolIsin)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Price).IsRequired();
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.Side).IsRequired();
            builder.Property(x => x.ValidityType).IsRequired();

            builder.Property(x => x.Commission)
                .HasPrecision(18, 8)
                .IsRequired();

            builder.Property(x => x.OrderModelType).IsRequired();
            builder.Property(x => x.OrderFrom).IsRequired();

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired(false);

            builder.HasIndex(x => x.SymbolIsin)
                .IsUnique()
                .HasDatabaseName("IX_TraderSymbol_SymbolIsin");
        }
    }
}