using Core.Infrastructure.Database.Configurations;
using EPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPM.Infrastructure.Configurations
{
   
    public class UserConfiguration : BaseConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
            builder.ToTable("User", "epm");

            builder.HasIndex(e => e.UserName, "UNQ_UserName").IsUnique();

            builder.Property(e => e.PasswordHash).IsUnicode(false);
            builder.Property(e => e.Phone)
                .HasMaxLength(13)
                .IsUnicode(false)
                .IsFixedLength();
            builder.Property(e => e.UserName)
                .HasMaxLength(500)
                .IsUnicode(false);

            builder.HasOne(d => d.FkLkpWorkCalendar).WithMany(p => p.Users)
                .HasForeignKey(d => d.FkLkpWorkCalendarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_LookUpValue");
            builder.HasIndex(e => e.FkLkpWorkCalendarId, "IX_User_FkLkpWorkCalendarId");

        }
    }
}

