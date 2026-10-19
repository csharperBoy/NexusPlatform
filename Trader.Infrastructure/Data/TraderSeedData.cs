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
using Microsoft.Extensions.Logging;

namespace Trader.Infrastructure.Data
{
    public static class TraderSeedData
    {
        /* ═══════════════════ Resources ═══════════════════ */

        private static List<ResourceDto> GetTraderResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "trader",
                    Name = "Trader",
                    Type = ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Trader management module",
                    DisplayOrder = 3000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        /* ─── Existing ─── */
                        new()
                        {
                            Key = "trader.account",
                            Name = "Trader Account",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Trader Account management",
                            DisplayOrder = 3001,
                            Icon = "list",
                        },
                        new()
                        {
                            // Legacy — باقی می‌ماند برای backward compat
                            Key = "trader.symbol",
                            Name = "Trader Symbol",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Trader Symbol management (legacy)",
                            DisplayOrder = 3002,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "trader.scheduleplan",
                            Name = "SchedulePlan",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "SchedulePlan management",
                            DisplayOrder = 3003,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "trader.scheduledorder",
                            Name = "ScheduledOrder",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "ScheduledOrder management",
                            DisplayOrder = 3004,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "trader.serverclock",
                            Name = "Server Clock",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Server clock management",
                            DisplayOrder = 3005,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "trader.executionlog",
                            Name = "ExecutionLog",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "ExecutionLog management",
                            DisplayOrder = 3006,
                            Icon = "list",
                        },

                        /* ─── New: Trading System ─── */
                        new()
                        {
                            Key = "trader.instrument",
                            Name = "Instrument",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Instrument management (Stocks, Options)",
                            DisplayOrder = 3007,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "trader.orderpreset",
                            Name = "Order Preset",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Default order settings per instrument",
                            DisplayOrder = 3008,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "trader.strategy",
                            Name = "Strategy",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Trading strategy registry",
                            DisplayOrder = 3009,
                            Icon = "activity",
                        },
                        new()
                        {
                            Key = "trader.tradingrun",
                            Name = "Trading Run",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Algorithmic trading runs",
                            DisplayOrder = 3010,
                            Icon = "play",
                        },
                        new()
                        {
                            Key = "trader.position",
                            Name = "Position",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Open positions and PnL",
                            DisplayOrder = 3011,
                            Icon = "bar-chart",
                        },
                        new()
                        {
                            Key = "trader.signal",
                            Name = "Signal",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Generated trading signals",
                            DisplayOrder = 3012,
                            Icon = "zap",
                        },
                    }
                }
            };
        }

        /* ═══════════════════ Permissions ═══════════════════ */

        private static List<PermissionDto> GetTraderPermissionDefinitions(Guid roleId)
        {
            // Helper برای ساخت تکراری‌ها
            static PermissionDto FullAccess(string resourceKey, Guid roleId, string description)
                => new()
                {
                    ResourceKey = resourceKey,
                    Action = PermissionAction.Full,
                    Scopes = new List<ScopeDto> { new() { scope = ScopeType.All } },
                    Effect = PermissionEffect.allow,
                    AssigneeType = AssigneeType.Role,
                    AssigneeId = roleId,
                    Description = description
                };

            return new List<PermissionDto>
            {
                // ─── Existing ───
                FullAccess("trader.account", roleId, "Full access to Trader Account"),
                FullAccess("trader.symbol", roleId, "Full access to Trader Symbol (legacy)"),
                FullAccess("trader.scheduleplan", roleId, "Full access to SchedulePlan"),
                FullAccess("trader.scheduledorder", roleId, "Full access to ScheduledOrder"),
                FullAccess("trader.serverclock", roleId, "Full access to Server Clock"),
                FullAccess("trader.executionlog", roleId, "Full access to Execution Log"),

                // ─── New ───
                FullAccess("trader.instrument", roleId, "Full access to Instrument"),
                FullAccess("trader.orderpreset", roleId, "Full access to Order Preset"),
                FullAccess("trader.strategy", roleId, "View strategies"),
                FullAccess("trader.tradingrun", roleId, "Full access to Trading Run"),
                FullAccess("trader.position", roleId, "View positions"),
                FullAccess("trader.signal", roleId, "View signals"),
            };
        }

        /* ═══════════════════ Seed: Authorization ═══════════════════ */

        public static async Task SeedTraderForAuthorizationAsync(
            IResourcePublicService resourcePublicService,
            IPermissionPublicService permissionPublicService,
            IRolePublicService roleService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Trader module seeding (Authorization)...");

            try
            {
                if (!ModuleHelper.IsActive(ModuleEnum.Trader))
                {
                    logger.LogInformation(
                        "Trader module is disabled; skipping authorization seed.");
                    return;
                }

                // 1. Resources
                var resources = GetTraderResourceDefinitions();
                await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                logger.LogInformation("✅ Trader resources synced.");

                // 2. Permissions
                var adminRoleId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

                var permissions = GetTraderPermissionDefinitions(adminRoleId);
                await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                logger.LogInformation("✅ Trader permissions seeded.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Trader authorization seeding");
                throw;
            }
        }

        /* ═══════════════════ Menus ═══════════════════ */

        private static List<MenuDto> GetTraderMenuDefinitions()
        {
            return new List<MenuDto>
            {
                new()
                {
                    Title = "مدیریت معاملات",
                    Description = "مدیریت معاملات الگوریتمی",
                    Icon = Icon.Folder.GetIconString(),
                    Order = 100,
                    Key = "Trader",
                    ParentKey = null,
                    Path = "/trader",
                    Children = new List<MenuDto>
                    {
                        new()
                        {
                            Title = "حساب‌های معاملاتی",
                            Description = "مدیریت حساب‌های کارگزاری",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 101,
                            Key = "trader.account",
                            ParentKey = "trader",
                            Path = "/trader/accounts"
                        },
                        new()
                        {
                            Title = "نمادها",
                            Description = "مدیریت نمادهای قابل معامله",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 102,
                            Key = "trader.instrument",
                            ParentKey = "trader",
                            Path = "/trader/instruments"
                        },
                        new()
                        {
                            Title = "تنظیمات پیش‌فرض سفارش",
                            Description = "پیش‌فرض‌های سفارش هر نماد",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 103,
                            Key = "trader.orderpreset",
                            ParentKey = "trader",
                            Path = "/trader/order-presets"
                        },
                        new()
                        {
                            Title = "استراتژی‌ها",
                            Description = "لیست استراتژی‌های قابل استفاده",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 104,
                            Key = "trader.strategy",
                            ParentKey = "trader",
                            Path = "/trader/strategies"
                        },
                        new()
                        {
                            Title = "پلن‌های معاملاتی",
                            Description = "Trading Runs — اجرای استراتژی",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 105,
                            Key = "trader.tradingrun",
                            ParentKey = "trader",
                            Path = "/trader/trading-runs"
                        },
                        new()
                        {
                            Title = "موقعیت‌های باز",
                            Description = "Positionهای باز و PnL",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 106,
                            Key = "trader.position",
                            ParentKey = "trader",
                            Path = "/trader/positions"
                        },
                        new()
                        {
                            Title = "سیگنال‌ها",
                            Description = "سیگنال‌های تولیدشده",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 107,
                            Key = "trader.signal",
                            ParentKey = "trader",
                            Path = "/trader/signals"
                        },
                        new()
                        {
                            Title = "پلن‌های زمان‌بندی‌شده",
                            Description = "سرخطی و سفارش‌های زمان‌بندی‌شده",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 108,
                            Key = "trader.scheduleplan",
                            ParentKey = "trader",
                            Path = "/trader/schedule-plans"
                        },
                        new()
                        {
                            Title = "زمان سرور",
                            Description = "همگام‌سازی زمان با سرور کارگزاری",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 109,
                            Key = "trader.serverclock",
                            ParentKey = "trader",
                            Path = "/trader/server-clock"
                        },
                        new()
                        {
                            Title = "لاگ اجرا",
                            Description = "لاگ‌های اجرای پلن‌ها و خطاها",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 110,
                            Key = "trader.executionlog",
                            ParentKey = "trader",
                            Path = "/trader/logs"
                        }
                    }
                }
            };
        }

        public static async Task SeedTradersForNavigationAsync(
            IMenuPublicService menuPublicService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Trader module seeding (Navigation)...");

            try
            {
                if (!ModuleHelper.IsActive(ModuleEnum.Trader))
                {
                    logger.LogInformation(
                        "Trader module is disabled; skipping navigation seed.");
                    return;
                }

                var menus = GetTraderMenuDefinitions();
                await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
                logger.LogInformation("✅ Trader menus synced.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Trader navigation seeding");
                throw;
            }
        }
    }
}