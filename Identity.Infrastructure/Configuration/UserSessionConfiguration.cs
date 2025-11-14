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
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> b)
        {
            b.ToTable("UserSessions", "identity");

            b.HasKey(s => s.Id);

            b.Property(s => s.RefreshToken)
                .IsRequired()
                .HasMaxLength(450);

            b.Property(s => s.DeviceInfo).HasMaxLength(500);
            b.Property(s => s.IpAddress).HasMaxLength(100);

            b.Property(s => s.ExpiresAt).IsRequired();

            b.Property(s => s.CreatedAt);
            //b.Property(s => s.LastModifiedOn);

            b.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
