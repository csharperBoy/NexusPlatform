using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Helper;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Data
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

    public static class HRSeedData
    {
        // تعریف ساختار درختی منابع ماژول HR
        private static List<ResourceDto> GetHrResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "hr",
                    Name = "HR",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "HR management module",
                    DisplayOrder = 3000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "hr.post",
                            Name = "HR Posts",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Post management",
                            DisplayOrder = 3001,
                            Icon = "list",
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول HR
        private static List<PermissionDto> GetHrPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDto>
            {
               new()
               {
                   ResourceKey = "hr.post",
                   Action = PermissionAction.Full,
                   Scopes = new List<ScopeDto>()
                   {
                       new()
                       {
                           scope =ScopeType.All
                       }
                   },
                   Effect = PermissionEffect.allow,
                   AssigneeType= AssigneeType.Role,
                   AssigneeId = roleId,

                   Description = "Full access to hr post"
               }
            };
        }
        public static async Task SeedHrForAuthorizationAsync(
          IResourcePublicService resourcePublicService,
          IPermissionPublicService permissionPublicService,
          IRolePublicService roleService,
          ILogger logger,
          CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting HR module seeding...");

            try
            {
                if (ModuleHelper.IsActive(ModuleEnum.HR))
                {
                    // 1. ثبت منابع (Resources)
                    // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                    var resources = GetHrResourceDefinitions();
                    await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                    logger.LogInformation("✅ HR resources synced successfully.");

                    // 2. ثبت پرمیشن‌ها (Permissions)
                    // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                    //var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);
                    var adminRoleId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

                    var permissions = GetHrPermissionDefinitions(adminRoleId);
                    await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                    logger.LogInformation("✅ HR permissions seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during HR module seeding");
                throw;
            }
        }
       /* public static async Task SeedHrAsync(
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
