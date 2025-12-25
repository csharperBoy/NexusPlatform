using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Application.Interfaces;
using Sample.Domain.Entities;

namespace OrganizationManagement.Infrastructure.Data
{
    /*
     📌 SampleSeedData
     -----------------
     این کلاس مسئول درج داده‌های اولیه (Seed Data) در دیتابیس ماژول Sample است.
     Seed Data معمولاً برای تست، دمو یا مقداردهی اولیه سیستم استفاده می‌شود.

     ✅ نکات کلیدی:
     - از Repository و UnitOfWork استفاده می‌کنیم تا عملیات درج داده‌ها مطابق با معماری لایه‌ای انجام شود.
     - قبل از درج داده، بررسی می‌کنیم که آیا داده‌ی مورد نظر از قبل وجود دارد یا نه (ExistsAsync).
     - اگر داده وجود نداشت، داده‌های اولیه ساخته و درج می‌شوند.
     - در نهایت تغییرات با UnitOfWork ذخیره و Commit می‌شوند.
     - لاگ‌ها برای اطلاع از موفقیت یا وجود داده قبلی ثبت می‌شوند.

     🛠 جریان کار:
     1. بررسی وجود داده با مقدار property1 = "SeededValue1".
     2. اگر داده وجود نداشت:
        - ایجاد لیست SampleEntity با مقادیر اولیه.
        - درج داده‌ها با Repository.
        - ذخیره تغییرات با UnitOfWork.
        - ثبت لاگ موفقیت.
     3. اگر داده وجود داشت:
        - ثبت لاگ و صرف‌نظر از درج مجدد.

     📌 نتیجه:
     این کلاس نشان می‌دهد چطور می‌توان داده‌های اولیه را به صورت ایمن و استاندارد
     با استفاده از Repository + UnitOfWork درج کرد، بدون اینکه داده‌های تکراری ایجاد شوند.
    */

    public static class OrganizationManagementSeedData
    {
        public static async Task SeedEntityAsync(
            IRepository<SampleDbContext, SampleEntity, Guid> repository,
            IUnitOfWork<SampleDbContext> unitOfWork,
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
        }
    }
}
