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
    public class ResourceConfiguration : AuditableEntityConfiguration<Resource>
    {
        public override void Configure(EntityTypeBuilder<Resource> builder)
        {
            base.Configure(builder);

            builder.ToTable("Resources", "authorization");

            builder.HasKey(r => r.Id);
            builder.HasIndex(r => r.Id).IsUnique();

            // Properties
            builder.Property(r => r.Key)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Description)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(r => r.Type)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(r => r.Category)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(r => r.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(r => r.Icon)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(r => r.Route)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(r => r.ParentId)
                .IsRequired(false);

            builder.Property(r => r.Path)
                .HasMaxLength(1000)
                .IsRequired(false);

            // Indexes
            builder.HasIndex(r => r.Key)
                .HasDatabaseName("IX_Resources_Key")
                .IsUnique();

            builder.HasIndex(r => new { r.Category, r.IsActive })
                .HasDatabaseName("IX_Resources_Category_Active");

            builder.HasIndex(r => new { r.Type, r.IsActive })
                .HasDatabaseName("IX_Resources_Type_Active");

            builder.HasIndex(r => new { r.ParentId, r.DisplayOrder })
                .HasDatabaseName("IX_Resources_Parent_Order");

            builder.HasIndex(r => r.Path)
                .HasDatabaseName("IX_Resources_Path");

            // Self-referencing relationship
            builder.HasOne(r => r.Parent)
                .WithMany(r => r.Children)
                .HasForeignKey(r => r.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete for hierarchy

            // Navigation properties for collections are already configured by EF Core conventions
            // but we can add additional configuration if needed
        }
    }
}
