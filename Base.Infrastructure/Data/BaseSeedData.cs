
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

namespace Base.Infrastructure.Data
{
    public static class BaseSeedData
    {
        // تعریف ساختار درختی منابع ماژول Audit
        private static List<ResourceDto> GetAuditResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Audit
        private static List<PermissionDto> GetBasePermissionDefinitions(Guid roleId)
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
        public static async Task SeedBaseForAuthorizationAsync(
          IResourcePublicService resourcePublicService,
          IPermissionPublicService permissionPublicService,
          IRolePublicService roleService,
          ILogger logger,
          CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Base module seeding...");

            try
            {
                if (ModuleHelper.IsActive(ModuleEnum.Authorization))
                {
                    // 1. ثبت منابع (Resources)
                    // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                    var resources = GetAuditResourceDefinitions();
                    await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                    logger.LogInformation("✅ Base resources synced successfully.");

                    // 2. ثبت پرمیشن‌ها (Permissions)
                    // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                    //var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);
                    var adminRoleId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

                    var permissions = GetBasePermissionDefinitions(adminRoleId);
                    await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                    logger.LogInformation("✅ Base permissions seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Audit module seeding");
                throw;
            }
        }

        public static async Task SeedBaseAsync(
            IUnitOfWork<BaseDbContext> unitOfWork,
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
