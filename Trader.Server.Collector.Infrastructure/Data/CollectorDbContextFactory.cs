using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Server.Collector.Infrastructure.Data
{

    public class CollectorDbContextFactory : IDesignTimeDbContextFactory<CollectorDbContext>
    {
        public CollectorDbContext CreateDbContext(string[] args)
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
            var optionsBuilder = new DbContextOptionsBuilder<CollectorDbContext>();
            optionsBuilder.UseSqlServer(conn, b =>
            {
                // 📌 تعیین اسمبلی محل Migrationها
                b.MigrationsAssembly(typeof(CollectorDbContext).Assembly.GetName().Name);

                // 📌 تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                b.MigrationsHistoryTable("__CollectorMigrationsHistory", "Collector");
            });

            // 📌 ساخت نمونه DbContext با تنظیمات مشخص‌شده
            return new CollectorDbContext(optionsBuilder.Options);
        }
    }
}
