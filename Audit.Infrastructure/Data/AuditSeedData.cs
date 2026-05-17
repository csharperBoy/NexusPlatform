using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Base.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Application.Helper;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
using Core.Shared.DTOs.Base;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.Data
{

    public static class AuditSeedData
    {
        #region ForAuthorization

       
        // تعریف ساختار درختی منابع ماژول Audit
        private static List<ResourceDto> GetAuditResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "audit",
                    Name = "Audit",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Audit management module",
                    DisplayOrder = 2000,
                    Icon = "shield",
                    //Path = "/audit",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "audit.auditlog",
                            Name = "Audit Logs",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Audit log management",
                            DisplayOrder = 2001,
                            Icon = "list",
                            //Path = "/audit/logs"
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
                    ResourceKey = "audit.auditlog",
                    Action = PermissionAction.Full, // مطمئن شوید این Enum در Core به صورت String یا Enum در دسترس است
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

                    Description = "Full access to audit logs"
                }
            };
        }

        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedAuditsForAuthorizationAsync(
            IResourcePublicService resourcePublicService,
            IPermissionPublicService permissionPublicService,
            IRolePublicService roleService, 
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Audit module seeding...");

            try
            {
                if (ModuleHelper.IsActive(ModuleEnum.Authorization))
                {
                    // 1. ثبت منابع (Resources)
                    // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                    var resources = GetAuditResourceDefinitions();
                    await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                    logger.LogInformation("✅ Audit resources synced successfully.");

                    // 2. ثبت پرمیشن‌ها (Permissions)
                    // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                    var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);

                    var permissions = GetAuditPermissionDefinitions(adminRoleId);
                    await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                    logger.LogInformation("✅ Audit permissions seeded successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Audit module seeding");
                throw;
            }
        }
        #endregion

        #region ForAuthorization

        // تعریف ساختار درختی منابع ماژول Audit
        private static List<MenuDto> GetAuditMenuDefinitions()
        {
            return new List<MenuDto>
            {
                new()
                {
                    Title = "مدیریت لاگ",
                    Description = "مدیریت لاگ های سیستم",
                    Icon = Core.Shared.Enums.Base.Icon.Folder,
                    Order = 100,
                    Path = "/Audit",
                    Children = new List<MenuDto>
                    {
                        new()
                        {
                            Title = "مشاهده لاگ ها",
                            Description = "مشاهده لاگ های سیستم",
                            Icon = Core.Shared.Enums.Base.Icon.Folder,
                            Order = 101,
                            Path = "/Audit/Get"
                        }
                    }
                }
            };
        }

       
        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedAuditsForBaseAsync(
            IMenuPublicService menuPublicService,           
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Audit module Fot Base seeding...");

            try
            {
               
                    // 1. ثبت منو (Menus)
                    var menus = GetAuditMenuDefinitions();
                    await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
                    logger.LogInformation("✅ Audit Menu synced successfully.");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Audit module seeding");
                throw;
            }
        }
        #endregion
    }
}
