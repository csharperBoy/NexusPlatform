using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Core.Domain.Enums;
using Core.Shared.DTOs.Authorization;
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
        public static async Task SeedAsync(
            IResourcePublicService resourcePublicService,
            IPermissionPublicService permissionPublicService,
            IRolePublicService roleService, 
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Audit module seeding...");

            try
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
                await permissionPublicService.SeedRolePermissionsAsync( permissions, cancellationToken);
                logger.LogInformation("✅ Audit permissions seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Audit module seeding");
                throw;
            }
        }
    }
}
