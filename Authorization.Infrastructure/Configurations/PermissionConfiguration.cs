using Authorization.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Configurations
{
    public class PermissionConfiguration : BaseConfiguration<Permission>
    {
        public override void Configure(EntityTypeBuilder<Permission> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("Permissions", "authorization");
            builder.HasIndex(e => e.CreatedAt, "IX_Permission_CreatedAt");
            builder.HasIndex(e => e.CreatedBy, "IX_Permission_CreatedBy");
            builder.HasIndex(e => e.ModifiedAt, "IX_Permission_ModifiedAt");
            builder.HasIndex(e => e.ModifiedBy, "IX_Permission_ModifiedBy");
            builder.HasIndex(e => e.OwnerOrganizationUnitId, "IX_Permission_OwnerOrgUnit");
            builder.HasIndex(e => e.OwnerPersonId, "IX_Permission_OwnerPerson");
            builder.HasIndex(e => new { e.OwnerOrganizationUnitId, e.OwnerPersonId }, "IX_Permission_ScopedLookup");
            builder.HasIndex(e => new { e.FkPermissionAssigneeId, e.FkResourceId, e.Action }, "IX_Permissions_FastLookup");
            builder.Property(e => e.Id).ValueGeneratedNever();
            builder.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            builder.Property(e => e.CreatedBy).HasMaxLength(256);
            builder.Property(e => e.ModifiedBy).HasMaxLength(256);
            builder.HasOne(d => d.PermissionAssignee).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.FkPermissionAssigneeId)
                .HasConstraintName("FK_Permissions_PermissionAssignee");

            builder.HasOne(d => d.Resource).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.FkResourceId)
                .HasConstraintName("FK_Permissions_Resources_ResourceId");
        }
    }
}
