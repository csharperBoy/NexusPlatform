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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> b)
        {
            b.ToTable("AspNetUsers", "identity");

            // EF Core Identity index configurations
            b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");

            // Person link
            b.HasIndex(u => u.FkPersonId).IsUnique();
            b.Property(u => u.FkPersonId).IsRequired();

            // Audit fields
            b.Property(u => u.CreatedAt).IsRequired();
            b.Property(u => u.UpdatedAt);

            // ValueObject: FullName
            b.OwnsOne(u => u.FullName, fn =>
            {
                fn.Property(f => f.FirstName)
                  .HasColumnName("FirstName")
                  .HasMaxLength(100);

                fn.Property(f => f.LastName)
                  .HasColumnName("LastName")
                  .HasMaxLength(100);
            });

            // One-to-many RefreshTokens
            b.HasMany(u => u.RefreshTokens)
             .WithOne(t => t.User)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // One-to-many Sessions
            b.HasMany(u => u.Sessions)
             .WithOne(s => s.User)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
