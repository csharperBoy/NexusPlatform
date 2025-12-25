using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Infrastructure.Database.Configurations;
using OrganizationManagement.Domain.Entities;

namespace OrganizationManagement.Infrastructure.Services
{
    
    }
    public class OrganizationUnitConfiguration : AuditableEntityConfiguration<OrganizationUnit>
    {
        public override void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            base.Configure(builder);
            builder.ToTable("OrganizationUnits", "organization");

            builder.Property(ou => ou.Name).IsRequired().HasMaxLength(200);
            builder.Property(ou => ou.Code).IsRequired().HasMaxLength(50);

            // مسیر سلسله‌مراتبی (مثلا: /1/5/12/) برای کوئری‌های سریع
            builder.Property(ou => ou.Path).HasMaxLength(1000).IsUnicode(false);

            // ایندکس‌ها
            builder.HasIndex(ou => ou.Code).IsUnique();
            builder.HasIndex(ou => ou.Path); // بسیار حیاتی برای Scoping

            // رابطه خود-ارجاعی
            builder.HasOne(ou => ou.Parent)
                .WithMany(ou => ou.Children)
                .HasForeignKey(ou => ou.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
