using Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> b)
        {
            b.ToTable("Permissions", "authorization");

            b.HasKey(x => x.Id);
            b.Property(x => x.Code).IsRequired().HasMaxLength(255);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasIndex(x => x.Code).IsUnique();

            b.HasOne(x => x.Resource)
             .WithMany(r => r.Permissions)
             .HasForeignKey(x => x.ResourceId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
