using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Trader.Server.Collector.Infrastructure.Data
{
 
    public static class CollectorSeedData
    {
        /*
        public static async Task SeedEntityAsync(
            IRepository<CollectorDbContext, CollectorEntity, Guid> repository,
            IUnitOfWork<CollectorDbContext> unitOfWork,
            IConfiguration config,
            ILogger logger)
        {
            // 📌 بررسی وجود داده‌ی اولیه
            var exists = await repository.ExistsAsync(e => e.property1 == "SeededValue1");

            if (!exists)
            {
                // 📌 ایجاد داده‌های اولیه
                var Collectors = new List<CollectorEntity>
                {
                    new CollectorEntity { property1 = "SeededValue1" },
                    new CollectorEntity { property1 = "SeededValue2" }
                };

                // 📌 درج داده‌ها با Repository
                await repository.AddRangeAsync(Collectors);

                // 📌 ذخیره تغییرات با UnitOfWork
                await unitOfWork.SaveChangesAsync();

                // 📌 ثبت لاگ موفقیت
                logger.LogInformation("✅ Collector seed data inserted successfully via Repository + UnitOfWork.");
            }
            else
            {
                // 📌 اگر داده وجود داشت، صرف‌نظر از درج مجدد
                logger.LogInformation("ℹ️ Collector seed data already exists, skipping.");
            }
        }
        */
    }
}
