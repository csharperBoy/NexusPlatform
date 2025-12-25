using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Database.Configurations
{
    public abstract class DataScopedEntityConfiguration<T> : AuditableEntityConfiguration<T>
     where T : DataScopedEntity
    {
        public override void Configure(EntityTypeBuilder<T> builder)
        {
            // ابتدا تنظیمات Auditable (CreatedAt, CreatedBy, ...) اعمال می‌شود
            base.Configure(builder);

            // تنظیمات مربوط به Scoping
            builder.Property(e => e.OwnerOrganizationUnitId)
                .IsRequired(false); // بسته به بیزینس می‌تواند اجباری باشد

            builder.Property(e => e.OwnerPersonId)
                .IsRequired(false);

            // ایندکس‌های حیاتی برای Performance در زمان فیلتر کردن داده‌ها
            builder.HasIndex(e => e.OwnerOrganizationUnitId)
                .HasDatabaseName($"IX_{typeof(T).Name}_OwnerOrgUnit");

            builder.HasIndex(e => e.OwnerPersonId)
                .HasDatabaseName($"IX_{typeof(T).Name}_OwnerPerson");

            // ایندکس ترکیبی برای سرعت بیشتر در سناریوهای معمول
            builder.HasIndex(e => new { e.OwnerOrganizationUnitId, e.OwnerPersonId })
                .HasDatabaseName($"IX_{typeof(T).Name}_ScopedLookup");
        }
    }
}
