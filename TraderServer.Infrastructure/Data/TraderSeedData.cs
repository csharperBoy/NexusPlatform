using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TraderServer.Infrastructure.DependencyInjection
{
 
      public static class TraderSeedData
    {
        // تعریف ساختار درختی منابع ماژول Audit
        private static List<ResourceDto> GetTraderResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "trader",
                    Name = "Trader",
                    Type =ResourceType.Module,
                    Category =ResourceCategory.System,
                    Description = "Trader management module",
                    DisplayOrder = 2000,
                    Icon = "shield",
                    //Path = "/audit",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "trader.stock",
                            Name = "Stock",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System ,
                            Description = "Stock management",
                            DisplayOrder = 2001,
                            Icon = "list",
                            //Path = "/audit/logs"
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Audit
        private static List<PermissionDto> GetTraderPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDto>
            {
                new()
                {
                    ResourceKey = "trader.stock",
                    Action =PermissionAction.Full, 
                    Scopes = new List<ScopeDto>(){ 
                                new(){
                                    scope = ScopeType.All 
                                } 
                            },
                    Effect = PermissionEffect.allow,
                    AssigneeType= AssigneeType.Role,
                    AssigneeId = roleId,

                    Description = "Full access to trader.stock"
                }
            };
        }

        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedAsync(
            IResourcePublicService resourcePublicService,
            IPermissionPublicService permissionPublicService,
            IRolePublicService roleService, // سرویس عمومی برای گرفتن نقش‌ها
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Trader module seeding...");

            try
            {
                // 1. ثبت منابع (Resources)
                // منطق Flatten کردن و ذخیره در دیتابیس کاملاً به ماژول Authorization سپرده شده
                var resources = GetTraderResourceDefinitions();
                await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                logger.LogInformation("✅ Trader resources synced successfully.");

                // 2. ثبت پرمیشن‌ها (Permissions)
                // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);

                var permissions = GetTraderPermissionDefinitions(adminRoleId);
                await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                logger.LogInformation("✅ Trader permissions seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Trader module seeding");
                throw;
            }
        }
    }
}
