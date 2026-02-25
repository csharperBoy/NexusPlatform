using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TraderServer.Infrastructure.DependencyInjection
{
 
      public static class TraderSeedData
    {
        // تعریف ساختار درختی منابع ماژول Audit
        private static List<ResourceDefinition> GetTraderResourceDefinitions()
        {
            return new List<ResourceDefinition>
            {
                new()
                {
                    Key = "trader",
                    Name = "Trader",
                    Type = "Module",
                    Category = "System",
                    Description = "Trader management module",
                    Order = 2000,
                    Icon = "shield",
                    //Path = "/audit",
                    Children = new List<ResourceDefinition>
                    {
                        new()
                        {
                            Key = "trader.stock",
                            Name = "Stock",
                            Type = "Data",
                            Category = "System",
                            Description = "Stock management",
                            Order = 2001,
                            Icon = "list",
                            //Path = "/audit/logs"
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Audit
        private static List<PermissionDefinition> GetTraderPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDefinition>
            {
                new()
                {
                    ResourceKey = "trader.stock",
                    Action = "Full", // مطمئن شوید این Enum در Core به صورت String یا Enum در دسترس است
                    Scope = "All",
                    Effect = "allow",
                    AssignType="Role",
                    AssignId = roleId,

                    Description = "Full access to trader.stock"
                }
            };
        }

        // متد اصلی Seed که توسط اپلیکیشن صدا زده می‌شود
        public static async Task SeedAsync(
            IAuthorizeSeedService seedService,
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
                await seedService.SyncModuleResourcesAsync(resources, cancellationToken);
                logger.LogInformation("✅ Trader resources synced successfully.");

                // 2. ثبت پرمیشن‌ها (Permissions)
                // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);

                var permissions = GetTraderPermissionDefinitions(adminRoleId);
                await seedService.SeedRolePermissionsAsync(permissions, cancellationToken);
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
