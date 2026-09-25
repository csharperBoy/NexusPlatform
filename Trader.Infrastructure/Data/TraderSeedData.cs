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

namespace Trader.Infrastructure.Data
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

    public static class TraderSeedData
    {
        // تعریف ساختار درختی منابع ماژول Trader
        private static List<ResourceDto> GetTraderResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "trader",
                    Name = "Trader",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Trader management module",
                    DisplayOrder = 3000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "trader.account",
                            Name = "Trader Account",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Trader Account management",
                            DisplayOrder = 3001,
                            Icon = "list",
                        },new()
                        {
                            Key = "trader.symbol",
                            Name = "Trader Symbol",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Trader Symbol management",
                            DisplayOrder = 3002,
                            Icon = "list",
                        },new()
                        {
                            Key = "trader.scheduleplan",
                            Name = "SchedulePlan",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "SchedulePlan management",
                            DisplayOrder = 3003,
                            Icon = "list",
                        },new()
                        {
                            Key = "trader.scheduledorder",
                            Name = "ScheduledOrder",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "ScheduledOrder management",
                            DisplayOrder = 3004,
                            Icon = "list",
                        },new()
                        {
                            Key = "trader.executionlog",
                            Name = "ExecutionLog",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "ExecutionLog management",
                            DisplayOrder = 3004,
                            Icon = "list",
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Trader
        private static List<PermissionDto> GetTraderPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDto>
            {
               new()
               {
                   ResourceKey = "trader.account",
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

                   Description = "Full access to trader account"
               },
               new()
               {
                   ResourceKey = "trader.symbol",
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

                   Description = "Full access to trader symbol"
               },
               new()
               {
                   ResourceKey = "trader.scheduleplan",
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

                   Description = "Full access to trader scheduleplan"
               },
               new()
               {
                   ResourceKey = "trader.scheduledorder",
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

                   Description = "Full access to trader scheduledorder"
               },
               new()
               {
                   ResourceKey = "trader.executionlog",
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

                   Description = "Full access to trader executionlog"
               }
            };
        }
        public static async Task SeedTraderForAuthorizationAsync(
          IResourcePublicService resourcePublicService,
          IPermissionPublicService permissionPublicService,
          IRolePublicService roleService,
          ILogger logger,
          CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Trader module seeding...");

            try
            {
                if (ModuleHelper.IsActive(ModuleEnum.Trader))
                {
                    // 1. ثبت منابع (Resources)
                    // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                    var resources = GetTraderResourceDefinitions();
                    await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                    logger.LogInformation("✅ Trader resources synced successfully.");

                    // 2. ثبت پرمیشن‌ها (Permissions)
                    // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                    //var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);
                    var adminRoleId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

                    var permissions = GetTraderPermissionDefinitions(adminRoleId);
                    await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                    logger.LogInformation("✅ Trader permissions seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Trader module seeding");
                throw;
            }
        }

        #region For Navigation

        // تعریف ساختار درختی منابع ماژول Trader
        private static List<MenuDto> GetTraderMenuDefinitions()
        {
            return new List<MenuDto>
            {
                new()
                {
                    Title = "مدیریت معاملات",
                    Description = "مدیریت معاملات",
                    Icon = Icon.Folder.GetIconString(),
                    Order = 100,
                    Key = "Trader",
                    ParentKey = null,
                    Path = "/trader",
                    Children = new List<MenuDto>
                    {
                        new()
                        {
                            Title = "مدیریت حساب های معاملاتی",
                            Description = "مدیریت حساب های معاملاتی",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 101,
                            Key = "trader.account",
                            ParentKey = "trader",
                            Path = "/trader/accounts"
                        },
                        new()
                        {
                            Title = "مدیریت نمادها",
                            Description = "مدیریت نمادها",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 102,
                            Key = "trader.symbol",
                            ParentKey = "trader",
                            Path = "/trader/symbols"
                        },
                        new()
                        {
                            Title = "مدیریت پلن های زمانبندی شده",
                            Description = "مدیریت پلن های زمانبندی شده",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 103,
                            Key = "trader.scheduleplan",
                            ParentKey = "trader",
                            Path = "/trader/schedule-plans"
                        },
                        new()
                        {
                            Title = "مدیریت تاخیر زمان سرور",
                            Description = "مدیریت پلن های زمانبندی شده",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 104,
                            Key = "trader.serverclock",
                            ParentKey = "trader",
                            Path = "/trader/server-clock"
                        }
                    }
                }
            };
        }


        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedTradersForNavigationAsync(
            IMenuPublicService menuPublicService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Trader module Fot Navigation seeding...");

            try
            {

                // 1. ثبت منو (Menus)
                var menus = GetTraderMenuDefinitions();
                await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
                logger.LogInformation("✅ Trader Menu synced successfully.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Trader module seeding");
                throw;
            }
        }
        #endregion

        /* public static async Task SeedTraderAsync(
             IUnitOfWork<TraderDbContext> unitOfWork,
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
