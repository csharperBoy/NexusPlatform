using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Data
{
    public class AuthorizationDbContextFactory : IDesignTimeDbContextFactory<AuthorizationDbContext>
    {
        public AuthorizationDbContext CreateDbContext(string[] args)
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
            var optionsBuilder = new DbContextOptionsBuilder<AuthorizationDbContext>();
            optionsBuilder.UseSqlServer(conn, b =>
            {
                // 📌 تعیین اسمبلی محل Migrationها
                b.MigrationsAssembly(typeof(AuthorizationDbContext).Assembly.GetName().Name);

                // 📌 تعیین جدول تاریخچه Migrationها در اسکیمای "Authorization"
                b.MigrationsHistoryTable("__AuthorizationMigrationsHistory", "Authorization");
            });

            // 📌 ساخت نمونه DbContext با تنظیمات مشخص‌شده
            return new AuthorizationDbContext(optionsBuilder.Options);
        }
    }
}
