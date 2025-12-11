using Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Infrastructure.Database.Configurations;

namespace Authorization.Infrastructure.Configurations
{
    public class PermissionConfiguration : AuditableEntityConfiguration<Permission>
    {
        public override void Configure(EntityTypeBuilder<Permission> builder)
        {
            base.Configure(builder);

            builder.ToTable("Permissions", "authorization");

            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.Id).IsUnique();

            // Properties
            builder.Property(p => p.ResourceId)
                .IsRequired();

            builder.Property(p => p.AssigneeType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.AssigneeId)
                .IsRequired();

            builder.Property(p => p.Action)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.IsAllow)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.Priority)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(p => p.Condition)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(p => p.EffectiveFrom)
                .IsRequired(false);

            builder.Property(p => p.ExpiresAt)
                .IsRequired(false);

            builder.Property(p => p.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.Order)
                .IsRequired()
                .HasDefaultValue(0);

            // Computed properties
          /*  builder.Property(p => p.IsValid)
                .HasComputedColumnSql(
                    "CASE WHEN IsActive = 1 AND " +
                    "(ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE()) AND " +
                    "(EffectiveFrom IS NULL OR EffectiveFrom <= GETUTCDATE()) " +
                    "THEN 1 ELSE 0 END",
                    stored: true);*/

            // Indexes
            builder.HasIndex(p => new { p.ResourceId, p.AssigneeType, p.AssigneeId, p.Action })
                .HasDatabaseName("IX_Permissions_Resource_Assignee_Action")
                .IsUnique();

            builder.HasIndex(p => new { p.AssigneeType, p.AssigneeId })
                .HasDatabaseName("IX_Permissions_Assignee");

            builder.HasIndex(p => p.ResourceId)
                .HasDatabaseName("IX_Permissions_Resource");

            builder.HasIndex(p => new { p.AssigneeId, p.IsActive,  p.Priority })
                .HasDatabaseName("IX_Permissions_ActivePriority");

            builder.HasIndex(p => new { p.Action, p.ResourceId })
                .HasDatabaseName("IX_Permissions_Action_Resource");

            // Relationships
            builder.HasOne(p => p.Resource)
                .WithMany(r => r.Permissions)
                .HasForeignKey(p => p.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
