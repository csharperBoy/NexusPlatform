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

namespace Contact.Infrastructure.Data
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

    public static class ContactSeedData
    {
        // تعریف ساختار درختی منابع ماژول Contact
        private static List<ResourceDto> GetContactResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "contact",
                    Name = "Contact",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Contact management module",
                    DisplayOrder = 3000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "contact.post",
                            Name = "Contact Posts",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Post management",
                            DisplayOrder = 3001,
                            Icon = "list",
                        },new()
                        {
                            Key = "contact.employment",
                            Name = "Contact employments",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "employment management",
                            DisplayOrder = 3002,
                            Icon = "list",
                        },new()
                        {
                            Key = "contact.location",
                            Name = "Contact locations",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "location management",
                            DisplayOrder = 3003,
                            Icon = "list",
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Contact
        private static List<PermissionDto> GetContactPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDto>
            {
               new()
               {
                   ResourceKey = "contact.post",
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

                   Description = "Full access to contact post"
               },
               new()
               {
                   ResourceKey = "contact.employment",
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

                   Description = "Full access to contact employment"
               },
               new()
               {
                   ResourceKey = "contact.location",
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

                   Description = "Full access to contact location"
               }
            };
        }
        public static async Task SeedContactForAuthorizationAsync(
          IResourcePublicService resourcePublicService,
          IPermissionPublicService permissionPublicService,
          IRolePublicService roleService,
          ILogger logger,
          CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Contact module seeding...");

            try
            {
                if (ModuleHelper.IsActive(ModuleEnum.Contact))
                {
                    // 1. ثبت منابع (Resources)
                    // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                    var resources = GetContactResourceDefinitions();
                    await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                    logger.LogInformation("✅ Contact resources synced successfully.");

                    // 2. ثبت پرمیشن‌ها (Permissions)
                    // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                    //var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);
                    var adminRoleId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

                    var permissions = GetContactPermissionDefinitions(adminRoleId);
                    await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                    logger.LogInformation("✅ Contact permissions seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Contact module seeding");
                throw;
            }
        }

        #region For Navigation

        // تعریف ساختار درختی منابع ماژول Contact
        private static List<MenuDto> GetContactMenuDefinitions()
        {
            return new List<MenuDto>
            {
                new()
                {
                    Title = "مدیریت منابع انسانی",
                    Description = "مدیریت منابع انسانی",
                    Icon = Icon.Folder.GetIconString(),
                    Order = 100,
                    Key = "Contact",
                    ParentKey = null,
                    Path = "/contact",
                    Children = new List<MenuDto>
                    {
                        new()
                        {
                            Title = "مدیریت پست های سازمانی",
                            Description = "مدیریت پست های سازمانی",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 101,
                            Key = "contact.post",
                            ParentKey = "contact",
                            Path = "/contact/post"
                        },
                        new()
                        {
                            Title = "مدیریت کارمندان",
                            Description = "مدیریت کارمندان",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 102,
                            Key = "contact.employment",
                            ParentKey = "contact",
                            Path = "/contact/employment"
                        },
                        new()
                        {
                            Title = "مدیریت مکان ها",
                            Description = "مدیریت مکان ها",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 103,
                            Key = "contact.location",
                            ParentKey = "contact",
                            Path = "/contact/location"
                        }
                    }
                }
            };
        }


        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedContactsForNavigationAsync(
            IMenuPublicService menuPublicService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Contact module Fot Navigation seeding...");

            try
            {

                // 1. ثبت منو (Menus)
                var menus = GetContactMenuDefinitions();
                await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
                logger.LogInformation("✅ Contact Menu synced successfully.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Contact module seeding");
                throw;
            }
        }
        #endregion

        /* public static async Task SeedContactAsync(
             IUnitOfWork<ContactDbContext> unitOfWork,
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
