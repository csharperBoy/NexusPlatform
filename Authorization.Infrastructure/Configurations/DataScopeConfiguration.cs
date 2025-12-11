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
    public class DataScopeConfiguration : AuditableEntityConfiguration<DataScope>
    {
        public override void Configure(EntityTypeBuilder<DataScope> builder)
        {
            base.Configure(builder);

            builder.ToTable("DataScopes", "authorization");

            builder.HasKey(ds => ds.Id);
            builder.HasIndex(ds => ds.Id).IsUnique();

            // Properties
            builder.Property(ds => ds.ResourceId)
                .IsRequired();

            builder.Property(ds => ds.AssigneeType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ds => ds.AssigneeId)
                .IsRequired();

            builder.Property(ds => ds.Scope)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ds => ds.SpecificUnitId)
                .IsRequired(false);

            builder.Property(ds => ds.CustomFilter)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(ds => ds.Depth)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(ds => ds.EffectiveFrom)
                .IsRequired(false);

            builder.Property(ds => ds.ExpiresAt)
                .IsRequired(false);

            builder.Property(ds => ds.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(ds => ds.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Computed properties (if needed for querying)
          /*  builder.Property(ds => ds.IsValid)
                .HasComputedColumnSql(
                    "CASE WHEN IsActive = 1 AND " +
                    "(ExpiresAt IS NULL OR ExpiresAt > GETUTCDATE()) AND " +
                    "(EffectiveFrom IS NULL OR EffectiveFrom <= GETUTCDATE()) " +
                    "THEN 1 ELSE 0 END",
                    stored: true);*/

            // Indexes
            builder.HasIndex(ds => new { ds.ResourceId, ds.AssigneeType, ds.AssigneeId })
                .HasDatabaseName("IX_DataScopes_Resource_Assignee");

            builder.HasIndex(ds => new { ds.AssigneeType, ds.AssigneeId })
                .HasDatabaseName("IX_DataScopes_Assignee");

            builder.HasIndex(ds => ds.ResourceId)
                .HasDatabaseName("IX_DataScopes_Resource");

            builder.HasIndex(ds => new { ds.AssigneeId, ds.IsActive })
                .HasDatabaseName("IX_DataScopes_Active");

            builder.HasIndex(ds => new { ds.Scope, ds.SpecificUnitId })
                .HasDatabaseName("IX_DataScopes_Scope_SpecificUnit");

            // Relationships
            builder.HasOne(ds => ds.Resource)
                .WithMany(r => r.DataScopes)
                .HasForeignKey(ds => ds.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
