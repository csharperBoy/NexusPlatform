using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Configuration
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("RefreshTokens", "identity");

            b.HasKey(r => r.Id);

            b.Property(r => r.Token)
                .IsRequired()
                .HasMaxLength(450)
                .IsUnicode(false);

            b.HasIndex(r => r.Token).IsUnique(false);

            b.Property(r => r.CreatedAt);
            //b.Property(r => r.LastModifiedOn);

            b.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
