using Core.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Core.Infrastructure.Database.Configurations
{
    /*
     📌 OutboxMessageConfiguration
     -----------------------------
     این کلاس وظیفه‌ی پیکربندی موجودیت **OutboxMessage** در EF Core را بر عهده دارد.
     هدف آن تعریف قوانین نگاشت (Mapping) بین مدل دامنه و جدول دیتابیس است.

     ✅ نکات کلیدی:
     - ToTable("OutboxMessages", _schema):
       • موجودیت OutboxMessage در جدول OutboxMessages ذخیره می‌شود.
       • امکان تعیین Schema برای جداسازی منطقی جداول.

     - HasKey(x => x.Id):
       • کلید اصلی جدول، پراپرتی Id است (از BaseEntity به ارث رسیده).

     - Property Configurations:
       • TypeName → حداکثر طول 255 کاراکتر، الزامی.
       • AssemblyQualifiedName → حداکثر طول 1024 کاراکتر، الزامی.
       • Content → الزامی (محتوای سریال‌شده رویداد).
       • OccurredOnUtc → الزامی (زمان وقوع رویداد).
       • ProcessedOnUtc → اختیاری (زمان پردازش).
       • Status → ذخیره به صورت int (Enum Conversion)، الزامی.
       • RetryCount → الزامی (تعداد تلاش مجدد).
       • ErrorMessage → حداکثر طول 2000 کاراکتر.
       • ErrorStackTrace → حداکثر طول 2000 کاراکتر.
       • RowVersion → Concurrency Token برای مدیریت همزمانی (Optimistic Concurrency).

     - Indexes:
       • ترکیبی روی Status و OccurredOnUtc → برای پردازش سریع پیام‌های Pending.
       • روی ProcessedOnUtc → برای گزارش‌گیری و مانیتورینگ.
       • روی TypeName → برای جستجوی سریع بر اساس نوع رویداد.

     🛠 جریان کار:
     1. هنگام اجرای Migration، این پیکربندی به دیتابیس اعمال می‌شود.
     2. جدول OutboxMessages با ستون‌ها و محدودیت‌های مشخص ایجاد می‌شود.
     3. OutboxProcessor می‌تواند با استفاده از ایندکس‌ها پیام‌ها را سریع‌تر پیدا کند.
     4. Concurrency Token تضمین می‌کند که دو پردازش همزمان روی یک پیام باعث Conflict نشود.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **EF Core Configuration for Outbox Pattern** است
     و تضمین می‌کند که پیام‌های Outbox به صورت پایدار، ایمن و بهینه در دیتابیس ذخیره شوند.
    */

    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        private readonly string _schema;

        public OutboxMessageConfiguration(string schema)
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages", _schema);
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TypeName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.AssemblyQualifiedName)
                .HasMaxLength(1024)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.OccurredOnUtc)
                .IsRequired();

            builder.Property(x => x.ProcessedOnUtc);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.RetryCount)
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(x => x.ErrorStackTrace)
                .HasMaxLength(2000);

            // 📌 Concurrency token برای مدیریت همزمانی
            builder.Property(x => x.RowVersion)
                   .IsRowVersion()
                   .IsConcurrencyToken();

            // 📌 ایندکس‌ها برای بهینه‌سازی Queryها
            builder.HasIndex(x => new { x.Status, x.OccurredOnUtc });
            builder.HasIndex(x => x.ProcessedOnUtc);
            builder.HasIndex(x => x.TypeName);
        }
    }
}
