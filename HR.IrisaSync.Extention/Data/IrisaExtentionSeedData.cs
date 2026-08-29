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

namespace HR.IrisaSync.Extention.Data
{


    public static class IrisaExtentionSeedData
    {
        #region ForAuthorization

        // تعریف ساختار درختی منابع ماژول HR
        private static List<ResourceDto> GetHrResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "hr.irisasync",
                    Name = "HR Irisa Sync Extention",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "HR Irisa Sync Extention management module",
                    DisplayOrder = 4000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "hr.irisasync.sync",
                            Name = "HR Irisa Sync",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Sync management",
                            DisplayOrder = 4001,
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
                   ResourceKey = "hr.irisasync.sync",
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

                   Description = "Full access to hr irisasync sync"
               }
            };
        }
        public static async Task SeedHrSyncForAuthorizationAsync(
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

        #endregion
        #region For Navigation

        // تعریف ساختار درختی منابع ماژول Hr
        private static List<MenuDto> GetHrMenuDefinitions()
        {
            return new List<MenuDto>
            {

                        new()
                        {
                            Title = "همگام سازی",
                            Description = "همگام سازی با سایر سیستم ها",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 101,
                            Key = "hr.sync",
                            ParentKey = "hr",
                            Path = "/hr/sync"
                        }

            };
        }


        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedHrSyncForNavigationAsync(
            IMenuPublicService menuPublicService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Hr module Fot Navigation seeding...");

            try
            {

                // 1. ثبت منو (Menus)
                var menus = GetHrMenuDefinitions();
                await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
                logger.LogInformation("✅ Hr Menu synced successfully.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Hr module seeding");
                throw;
            }
        }
        #endregion
        /*
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
