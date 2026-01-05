using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Core.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.Data
{

    public static class AuditSeedData
    {
        // تعریف ساختار درختی منابع ماژول Audit
        private static List<ResourceDefinition> GetAuditResourceDefinitions()
        {
            return new List<ResourceDefinition>
            {
                new()
                {
                    Key = "audit",
                    Name = "Audit",
                    Type = "Module",
                    Category = "System",
                    Description = "Audit management module",
                    Order = 2000,
                    Icon = "shield",
                    Path = "/audit",
                    Children = new List<ResourceDefinition>
                    {
                        new()
                        {
                            Key = "audit.auditlog",
                            Name = "Audit Logs",
                            Type = "Data",
                            Category = "System",
                            Description = "Audit log management",
                            Order = 2001,
                            Icon = "list",
                            Path = "/audit/logs"
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Audit
        private static List<PermissionDefinition> GetAuditPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDefinition>
            {
                new()
                {
                    ResourceKey = "audit.auditlog",
                    Action = "Full", // مطمئن شوید این Enum در Core به صورت String یا Enum در دسترس است
                    Scope = "All",
                    Type = "allow",
                    AssignType="Role",
                    AssignId = roleId,

                    Description = "Full access to audit logs"
                }
            };
        }

        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedAsync(
            IResourcePublicService resourceService,
            IPermissionPublicService permissionService,
            IRolePublicService roleService, // سرویس عمومی برای گرفتن نقش‌ها
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Audit module seeding...");

            try
            {
                // 1. ثبت منابع (Resources)
                // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                var resources = GetAuditResourceDefinitions();
                await resourceService.SyncModuleResourcesAsync(resources, cancellationToken);
                logger.LogInformation("✅ Audit resources synced successfully.");

                // 2. ثبت پرمیشن‌ها (Permissions)
                // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);

                var permissions = GetAuditPermissionDefinitions(adminRoleId);
                await permissionService.SeedRolePermissionsAsync( permissions, cancellationToken);
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
