using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Infrastructure.Data
{
    /*
     📌 SampleDbContextFactory
     -------------------------
     این کلاس یک Factory برای ایجاد DbContext در زمان طراحی (Design-Time) است.
     EF Core برای عملیات‌هایی مثل **Migrations** و **Update-Database** نیاز دارد
     که بتواند یک نمونه از DbContext را بدون اجرای کل برنامه بسازد.

     ✅ نکات کلیدی:
     - از IDesignTimeDbContextFactory<SampleDbContext> ارث‌بری می‌کند.
     - متد CreateDbContext توسط EF Core در زمان طراحی فراخوانی می‌شود.
     - فایل‌های تنظیمات (appsettings.json و appsettings.Development.json) خوانده می‌شوند
       تا Connection String استخراج شود.
     - DbContextOptionsBuilder برای تنظیمات EF Core استفاده می‌شود:
       1. UseSqlServer → اتصال به SQL Server.
       2. MigrationsAssembly → تعیین اسمبلی محل نگهداری Migrationها.
       3. MigrationsHistoryTable → تعیین جدول تاریخچه Migrationها با نام سفارشی.

     🛠 جریان کار:
     1. EF Core دستور `dotnet ef migrations add` یا `dotnet ef database update` را اجرا می‌کند.
     2. این Factory فراخوانی می‌شود تا یک نمونه از SampleDbContext ساخته شود.
     3. تنظیمات اتصال به دیتابیس از فایل‌های پیکربندی خوانده می‌شود.
     4. DbContext ساخته شده و EF Core می‌تواند Migrationها را اعمال کند.

     📌 نتیجه:
     این کلاس تضمین می‌کند که EF Core همیشه می‌تواند در زمان طراحی یک DbContext معتبر بسازد،
     حتی بدون اجرای کل برنامه. این کار برای مدیریت Migrationها ضروری است.
    */

    public class OrganizationManagementDbContextFactory : IDesignTimeDbContextFactory<SampleDbContext>
    {
        public SampleDbContext CreateDbContext(string[] args)
        {
            // 📌 مسیر پایه برای خواندن فایل‌های تنظیمات
            var basePath = Directory.GetCurrentDirectory();

            // 📌 ساخت Configuration از فایل‌های appsettings و متغیرهای محیطی
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // 📌 گرفتن Connection String از تنظیمات
            var conn = config.GetConnectionString("DefaultConnection");

            // 📌 تنظیم DbContextOptions برای EF Core
            var optionsBuilder = new DbContextOptionsBuilder<SampleDbContext>();
            optionsBuilder.UseSqlServer(conn, b =>
            {
                // 📌 تعیین اسمبلی محل Migrationها
                b.MigrationsAssembly(typeof(SampleDbContext).Assembly.GetName().Name);

                // 📌 تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                b.MigrationsHistoryTable("__SampleMigrationsHistory", "sample");
            });

            // 📌 ساخت نمونه DbContext با تنظیمات مشخص‌شده
            return new SampleDbContext(optionsBuilder.Options);
        }
    }
}
