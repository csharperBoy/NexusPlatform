using Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Core.Infrastructure.Database.Configurations
{
    public abstract class AuditableEntityConfiguration<T> : IEntityTypeConfiguration<T>
        where T : AuditableEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
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
                .HasDatabaseName($"IX_{typeof(T).Name}_CreatedAt");

            builder.HasIndex(e => e.ModifiedAt)
                .HasDatabaseName($"IX_{typeof(T).Name}_ModifiedAt");

            builder.HasIndex(e => e.CreatedBy)
                .HasDatabaseName($"IX_{typeof(T).Name}_CreatedBy");

            builder.HasIndex(e => e.ModifiedBy)
                .HasDatabaseName($"IX_{typeof(T).Name}_ModifiedBy");
        }
    }
}
