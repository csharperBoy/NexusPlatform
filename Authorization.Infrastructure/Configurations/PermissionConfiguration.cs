using Authorization.Domain.Entities;
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

            // relation rule
            builder.HasMany(p => p.Rules)
                     .WithOne(pr => pr.Permission)
                     .HasForeignKey(pr => pr.PermissionId)
                     .OnDelete(DeleteBehavior.Cascade);

            // relation scope
            builder.HasMany(p => p.Scopes)
                     .WithOne(pr => pr.Permission)
                     .HasForeignKey(pr => pr.PermissionId)
                     .OnDelete(DeleteBehavior.Cascade);

            // استفاده از Byte برای Enumها جهت سرعت در محاسبات Permission
            builder.Property(p => p.AssigneeType).HasConversion<byte>();
            builder.Property(p => p.Action).HasConversion<byte>();
            

            // ایندکس ترکیبی طلایی برای چک کردن دسترسی
            builder.HasIndex(p => new { p.AssigneeId, p.ResourceId, p.Action })
                   .HasDatabaseName("IX_Permissions_FastLookup");

            // عدم ثبت دسترسی تکراری
            builder.HasIndex(p => new {
                p.ResourceId,
                p.Action,
                p.AssigneeType,
                p.AssigneeId,
                p.Effect
            })
            .HasDatabaseName("IX_Permissions_Unique")
            .IsUnique();
        }
    }
}
