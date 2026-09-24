using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations
{
    public class TraderAccountConfiguration : BaseConfiguration<TraderAccount>
    {
        public override void Configure(EntityTypeBuilder<TraderAccount> builder)
        {
            base.Configure(builder);
            builder.ToTable("Accounts", "trader");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.EncryptedPassword)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.EncryptedToken)
                .IsRequired(false)
                .HasMaxLength(4000);

            builder.Property(x => x.TokenExp)
                .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired(false);

            builder.HasIndex(x => x.Username)
                .HasDatabaseName("IX_TraderAccount_Username");
        }
    }
}