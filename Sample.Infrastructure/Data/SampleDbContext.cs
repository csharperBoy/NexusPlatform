using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sample.Domain;
using Sample.Domain.Entities;
namespace Sample.Infrastructure.Data
{
    /*
     📌 SampleDbContext
     ------------------
     این کلاس DbContext مخصوص ماژول Sample است و وظیفه‌اش مدیریت ارتباط با دیتابیس
     از طریق EF Core می‌باشد.

     ✅ نکات کلیدی:
     - از DbContext ارث‌بری می‌کند و در لایه Infrastructure قرار دارد.
     - شامل DbSetها برای موجودیت‌های دامنه است:
       1. SampleEntity → موجودیت اصلی ماژول Sample.
       2. OutboxMessage → برای پیاده‌سازی الگوی Outbox و ذخیره رویدادهای دامنه.
     - در متد OnModelCreating تنظیمات Fluent API اعمال می‌شود:
       - تعیین اسکیمای پیش‌فرض به نام "sample".
       - اعمال تنظیمات OutboxMessageConfiguration برای نگهداری Outbox.
       - تعریف کلید اصلی و محدودیت‌های property1 در SampleEntity.

     🛠 جریان کار:
     1. هنگام اجرای برنامه، EF Core از این DbContext برای ارتباط با دیتابیس استفاده می‌کند.
     2. DbSetها به جداول دیتابیس نگاشت می‌شوند.
     3. Fluent API در OnModelCreating قوانین نگاشت و محدودیت‌ها را مشخص می‌کند.
     4. OutboxMessageها در کنار موجودیت‌های دامنه ذخیره می‌شوند تا بعداً توسط OutboxProcessor منتشر شوند.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور باید DbContext اختصاصی برای هر ماژول تعریف کنیم،
     چطور موجودیت‌ها و Outbox را نگاشت کنیم،
     و چطور اسکیمای جداگانه برای ماژول‌ها داشته باشیم تا معماری ماژولار حفظ شود.
    */

    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

        // 📌 جدول مربوط به موجودیت SampleEntity
        public DbSet<SampleEntity> Sample { get; set; } = null!;

        // 📌 جدول مربوط به OutboxMessage برای ذخیره رویدادهای دامنه
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 📌 تعیین اسکیمای پیش‌فرض برای جداول این ماژول
            modelBuilder.HasDefaultSchema("sample");

            // 📌 اعمال تنظیمات OutboxMessageConfiguration
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("sample"));

            // 📌 تنظیمات Fluent API برای SampleEntity
            modelBuilder.Entity<SampleEntity>(entity =>
            {
                entity.HasKey(e => e.Id); // کلید اصلی
                entity.Property(e => e.property1)
                      .IsRequired()
                      .HasMaxLength(200); // محدودیت طول
            });
        }
    }
}
