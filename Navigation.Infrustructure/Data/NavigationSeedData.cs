using Navigation.Domain.Entities;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Helper;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Infrastructure.Data
{
    public static class NavigationSeedData
    {
        // تعریف ساختار درختی منابع ماژول Audit
        private static List<ResourceDto> GetAuditResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "base",
                    Name = "Navigation",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Navigation management module",
                    DisplayOrder = 2000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "base.menu",
                            Name = "Navigation Menus",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Menu management",
                            DisplayOrder = 2001,
                            Icon = "list",
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Audit
        private static List<PermissionDto> GetAuditPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDto>
            {
               new()
               {
                   ResourceKey = "base.menu",
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

                   Description = "Full access to base menu"
               }
            };
        }
        public static async Task SeedNavigationForAuthorizationAsync(
          IResourcePublicService resourcePublicService,
          IPermissionPublicService permissionPublicService,
          IRolePublicService roleService,
          ILogger logger,
          CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Navigation module seeding...");

            try
            {
                if (ModuleHelper.IsActive(ModuleEnum.Authorization))
                {
                    // 1. ثبت منابع (Resources)
                    // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                    var resources = GetAuditResourceDefinitions();
                    await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                    logger.LogInformation("✅ Navigation resources synced successfully.");

                    // 2. ثبت پرمیشن‌ها (Permissions)
                    // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                    var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);

                    var permissions = GetAuditPermissionDefinitions(adminRoleId);
                    await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                    logger.LogInformation("✅ Navigation permissions seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Audit module seeding");
                throw;
            }
        }

        public static async Task SeedNavigationAsync(
            IRepository<NavigationDbContext,Menu, Guid> menuRepository,
            IUnitOfWork<NavigationDbContext> unitOfWork,
            IConfiguration config,
            ILogger logger)
        {
            // 📌 بررسی وجود داده‌ی اولیه
            //var exists = await menuRepository.ExistsAsync(e => e.property1 == "SeededValue1");

            //if (!exists)
            //{
            //    // 📌 ایجاد داده‌های اولیه
            //    var bases = new List<BaseEntity>
            //    {
            //        new BaseEntity { property1 = "SeededValue1" },
            //        new BaseEntity { property1 = "SeededValue2" }
            //    };

            //    // 📌 درج داده‌ها با Repository
            //    await repository.AddRangeAsync(bases);

            //    // 📌 ذخیره تغییرات با UnitOfWork
            //    await unitOfWork.SaveChangesAsync();

            //    // 📌 ثبت لاگ موفقیت
            //    logger.LogInformation("✅ Base seed data inserted successfully via Repository + UnitOfWork.");
            //}
            //else
            //{
            //    // 📌 اگر داده وجود داشت، صرف‌نظر از درج مجدد
            //    logger.LogInformation("ℹ️ Base seed data already exists, skipping.");
            //}
        }
    }
}
