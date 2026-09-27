using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Abstractions.Navigation.PublicService;
using Core.Application.Helper;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
using Core.Shared.DTOs.Navigation;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.Navigation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Scheduler.Infrastructure.Data
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

    public static class SchedulerSeedData
    {
        // تعریف ساختار درختی منابع ماژول Scheduler
        private static List<ResourceDto> GetSchedulerResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "scheduler",
                    Name = "Scheduler",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Scheduler management module",
                    DisplayOrder = 4000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "scheduler.job",
                            Name = "Scheduler Jobs",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Job management",
                            DisplayOrder = 4001,
                            Icon = "list",
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Scheduler
        private static List<PermissionDto> GetSchedulerPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDto>
            {
              
               new()
               {
                   ResourceKey = "scheduler.job",
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

                   Description = "Full access to scheduler job"
               }
            };
        }
        public static async Task SeedSchedulerForAuthorizationAsync(
          IResourcePublicService resourcePublicService,
          IPermissionPublicService permissionPublicService,
          IRolePublicService roleService,
          ILogger logger,
          CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Scheduler module seeding...");

            try
            {
                if (ModuleHelper.IsActive(ModuleEnum.Scheduler))
                {
                    // 1. ثبت منابع (Resources)
                    // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                    var resources = GetSchedulerResourceDefinitions();
                    await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                    logger.LogInformation("✅ Scheduler resources synced successfully.");

                    // 2. ثبت پرمیشن‌ها (Permissions)
                    // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                    //var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);
                    var adminRoleId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

                    var permissions = GetSchedulerPermissionDefinitions(adminRoleId);
                    await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                    logger.LogInformation("✅ Scheduler permissions seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Scheduler module seeding");
                throw;
            }
        }

        #region For Navigation

        // تعریف ساختار درختی منابع ماژول Scheduler
        private static List<MenuDto> GetSchedulerMenuDefinitions()
        {
            return new List<MenuDto>
            {
                new()
                {
                    Title = "مدیریت برنامه های زمانبندی شده",
                    Description = "مدیریت برنامه های زمانبندی شده",
                    Icon = Icon.Folder.GetIconString(),
                    Order = 200,
                    Key = "Scheduler",
                    ParentKey = null,
                    Path = "/scheduler",
                    Children = new List<MenuDto>
                    {
                        new()
                        {
                            Title = "برنامه های زمان بندی شده",
                            Description = "برنامه های زمان بندی شده",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 201,
                            Key = "scheduler.job",
                            ParentKey = "scheduler",
                            Path = "/scheduler/job"
                        }
                    }
                }
            };
        }


        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedSchedulersForNavigationAsync(
            IMenuPublicService menuPublicService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Scheduler module Fot Navigation seeding...");

            try
            {

                // 1. ثبت منو (Menus)
                var menus = GetSchedulerMenuDefinitions();
                await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
                logger.LogInformation("✅ Scheduler Menu synced successfully.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Scheduler module seeding");
                throw;
            }
        }
        #endregion

        /* public static async Task SeedSchedulerAsync(
             IUnitOfWork<SchedulerDbContext> unitOfWork,
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
