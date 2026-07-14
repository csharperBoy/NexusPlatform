using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HR.IrisaSync.Extention.Data
{
    

    public static class IrisaExtentionSeedData
    {/*
        public static async Task SeedEntityAsync(
            IRepository<HRDbContext, SampleEntity, Guid> repository,
            IUnitOfWork<HRDbContext> unitOfWork,
            IConfiguration config,
            ILogger logger)
        {
            // 📌 بررسی وجود داده‌ی اولیه
            var exists = await repository.ExistsAsync(e => e.property1 == "SeededValue1");

            if (!exists)
            {
                // 📌 ایجاد داده‌های اولیه
                var samples = new List<SampleEntity>
                {
                    new SampleEntity { property1 = "SeededValue1" },
                    new SampleEntity { property1 = "SeededValue2" }
                };

                // 📌 درج داده‌ها با Repository
                await repository.AddRangeAsync(samples);

                // 📌 ذخیره تغییرات با UnitOfWork
                await unitOfWork.SaveChangesAsync();

                // 📌 ثبت لاگ موفقیت
                logger.LogInformation("✅ Sample seed data inserted successfully via Repository + UnitOfWork.");
            }
            else
            {
                // 📌 اگر داده وجود داشت، صرف‌نظر از درج مجدد
                logger.LogInformation("ℹ️ Sample seed data already exists, skipping.");
            }
        }*/
    }
}
