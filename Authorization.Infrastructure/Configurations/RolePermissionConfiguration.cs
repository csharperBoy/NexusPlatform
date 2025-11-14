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
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> b)
        {
            b.ToTable("RolePermissions", "authorization");

            b.HasKey(x => x.Id);
            b.Property(x => x.Scope).HasMaxLength(50).IsRequired(false);
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();

            b.HasOne(x => x.Permission)
             .WithMany(p => p.RolePermissions)
             .HasForeignKey(x => x.PermissionId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
