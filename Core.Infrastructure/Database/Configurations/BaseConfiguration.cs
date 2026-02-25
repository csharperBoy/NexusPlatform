using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Database.Configurations
{
    public class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : class
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ConfigureBaseEntity(builder as EntityTypeBuilder<BaseEntity>);
            }
            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ConfigureAuditable(builder as EntityTypeBuilder<IAuditableEntity>);
            }
            if (typeof(IDataScopedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ConfigureDataScoped(builder as EntityTypeBuilder<IDataScopedEntity>);
            }
        }
        private void ConfigureBaseEntity(EntityTypeBuilder<BaseEntity> builder)
        {

        }

        private void ConfigureAuditable(EntityTypeBuilder<IAuditableEntity> builder)
        {
            // تنظیمات مشترک برای همه Auditable Entities
            builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()"); // مقدار پیش‌فرض در دیتابیس

            builder.Property(e => e.CreatedBy)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(e => e.ModifiedAt)
                .IsRequired(false);

            builder.Property(e => e.ModifiedBy)
                .HasMaxLength(256)
                .IsRequired(false);

            // ایندکس‌های عملکردی
            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedAt");

            builder.HasIndex(e => e.ModifiedAt)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_ModifiedAt");

            builder.HasIndex(e => e.CreatedBy)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedBy");

            builder.HasIndex(e => e.ModifiedBy)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_ModifiedBy");
        }
        
        private void ConfigureDataScoped(EntityTypeBuilder<IDataScopedEntity> builder)
        {
            // تنظیمات مربوط به Scoping
            builder.Property(e => e.OwnerOrganizationUnitId)
                .IsRequired(false); // بسته به بیزینس می‌تواند اجباری باشد

            builder.Property(e => e.OwnerPersonId)
                .IsRequired(false);

            // ایندکس‌های حیاتی برای Performance در زمان فیلتر کردن داده‌ها
            builder.HasIndex(e => e.OwnerOrganizationUnitId)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_OwnerOrgUnit");

            builder.HasIndex(e => e.OwnerPersonId)
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_OwnerPerson");

            // ایندکس ترکیبی برای سرعت بیشتر در سناریوهای معمول
            builder.HasIndex(e => new { e.OwnerOrganizationUnitId, e.OwnerPersonId })
                .HasDatabaseName($"IX_{typeof(TEntity).Name}_ScopedLookup");
        }


    }
}
