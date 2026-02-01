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
    public class PermissionConfiguration : DataScopedEntityConfiguration<Permission>
    {
        public override void Configure(EntityTypeBuilder<Permission> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("Permissions", "authorization");

            // استفاده از Byte برای Enumها جهت سرعت در محاسبات Permission
            builder.Property(p => p.AssigneeType).HasConversion<byte>();
            builder.Property(p => p.Action).HasConversion<byte>();
            builder.Property(p => p.Scope).HasConversion<byte>();

            builder.Property(p => p.SpecificScopeId)
              .IsRequired();

            // ایندکس ترکیبی طلایی برای چک کردن دسترسی
            builder.HasIndex(p => new { p.AssigneeId, p.ResourceId, p.Action })
                   .HasDatabaseName("IX_Permissions_FastLookup");

            // عدم ثبت دسترسی تکراری
            builder.HasIndex(p => new {
                p.ResourceId,
                p.Scope,
                p.SpecificScopeId, 
                p.Action,
                p.AssigneeType,
                p.AssigneeId,
                p.Type
            })
            .HasDatabaseName("IX_Permissions_Unique")
            .IsUnique();
        }
    }
}
