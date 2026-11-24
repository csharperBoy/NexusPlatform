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
using Epm.Application.Abstractions;
using Epm.Domain.Entities;

namespace EPM.Infrastructure.Data
{
    public static class EpmSeedData
    {
        /* ═══════════════════ Resources ═══════════════════ */

        private static List<ResourceDto> GetEpmResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "epm",
                    Name = "Epm",
                    Type = ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Epm management module",
                    DisplayOrder = 3000,
                    Icon = "shield",
                    Children = new List<ResourceDto>
                    {
                        /* ─── Existing ─── */
                        new()
                        {
                            Key = "epm.category",
                            Name = "Epm category",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Epm category management",
                            DisplayOrder = 3001,
                            Icon = "list",
                        },
                        new()
                        {
                            // Legacy — باقی می‌ماند برای backward compat
                            Key = "epm.indicator",
                            Name = "Epm indicator",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Epm indicator management",
                            DisplayOrder = 3002,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "epm.claimonindicator",
                            Name = "claim on indicator",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "claim on indicator management",
                            DisplayOrder = 3003,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "epm.claimonsystem",
                            Name = "claim on system",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "claim on system management",
                            DisplayOrder = 3004,
                            Icon = "list",
                        },
                        new()
                        {
                            Key = "epm.user",
                            Name = "user",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "Epm user registry",
                            DisplayOrder = 3005,
                            Icon = "activity",
                        },
                        new()
                        {
                            Key = "epm.indicatorvalue",
                            Name = "indicator value ",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "indicator value",
                            DisplayOrder = 3006,
                            Icon = "play",
                        },
                        new()
                        {
                            Key = "epm.lookup",
                            Name = "lookup",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "lookup",
                            DisplayOrder = 3007,
                            Icon = "bar-chart",
                        },
                        new()
                        {
                            Key = "epm.mapitem",
                            Name = "mapitem",
                            Type = ResourceType.Data,
                            Category = ResourceCategory.System,
                            Description = "mapitem",
                            DisplayOrder = 30008,
                            Icon = "zap",
                        }
                    }
                }
            };
        }

        /* ═══════════════════ Permissions ═══════════════════ */

        private static List<PermissionDto> GetEpmPermissionDefinitions(Guid roleId)
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
                FullAccess("epm.user", roleId, "Full access to Epm Account"),
                FullAccess("epm.category", roleId, "Full access to Epm Symbol (legacy)"),
                FullAccess("epm.indicator", roleId, "Full access to SchedulePlan"),
                FullAccess("epm.indicatorvalue", roleId, "Full access to ScheduledOrder"),
                FullAccess("epm.lookup", roleId, "Full access to Server Clock"),
                FullAccess("epm.mapitem", roleId, "Full access to Execution Log"),
                FullAccess("epm.claimonsystem", roleId, "Full access to Instrument"),
                FullAccess("epm.claimonindicator", roleId, "Full access to Order Preset"),
            };
        }

        /* ═══════════════════ Seed: Authorization ═══════════════════ */

        public static async Task SeedEpmForAuthorizationAsync(
            IResourcePublicService resourcePublicService,
            IPermissionPublicService permissionPublicService,
            IRolePublicService roleService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Epm module seeding (Authorization)...");

            try
            {
                if (!ModuleHelper.IsActive(ModuleEnum.Epm))
                {
                    logger.LogInformation(
                        "Epm module is disabled; skipping authorization seed.");
                    return;
                }

                // 1. Resources
                var resources = GetEpmResourceDefinitions();
                await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                logger.LogInformation("✅ Epm resources synced.");

                // 2. Permissions
                var adminRoleId = await roleService.GetAdminRolePermissionAssigneeIdAsync(cancellationToken);

                var permissions = GetEpmPermissionDefinitions(adminRoleId);
                await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                logger.LogInformation("✅ Epm permissions seeded.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Epm authorization seeding");
                throw;
            }
        }

        /* ═══════════════════ Menus ═══════════════════ */

        private static List<MenuDto> GetEpmMenuDefinitions()
        {
            return new List<MenuDto>
            {
                new()
                {
                    Title = "مدیریت عملکرد سازمانی",
                    Description = "مدیریت و پایش عملکرد سازمانی",
                    Icon = Icon.Folder.GetIconString(),
                    Order = 100,
                    Key = "epm",
                    ParentKey = null,
                    Path = "/epm",
                    Children = new List<MenuDto>
                    {
                        new()
                        {
                            Title = "شناسنامه شاخص",
                            Description = "مدیریت شناسنامه شاخص",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 101,
                            Key = "epm.indicator",
                            ParentKey = "epm",
                            Path = "/epm/indicator"
                        },
                        new()
                        {
                            Title = "دسته‌بندی‌ها",
                            Description = "مدیریت دسته‌بندی‌ها",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 102,
                            Key = "epm.category",
                            ParentKey = "epm",
                            Path = "/epm/category"
                        },
                        new()
                        {
                            Title = "مجوزها",
                            Description = "مدیریت مجوزها",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 103,
                            Key = "epm.claim",
                            ParentKey = "epm",
                            Path = "/epm/claim"
                        },
                        new()
                        {
                            Title = "ورود مقادیر",
                            Description = "ورود مقادیر",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 104,
                            Key = "epm.indicatorvalue",
                            ParentKey = "epm",
                            Path = "/epm/indicatorvalue"
                        },
                        new()
                        {
                            Title = "گزارشات'",
                            Description = "گزارشات",
                            Icon = Icon.Folder.GetIconString(),
                            Order = 105,
                            Key = "epm.report",
                            ParentKey = "epm",
                            Path = "/epm/report",
                            Children = new List<MenuDto>
                            {
                                new()
                                {
                                    Title = "گزارش شناسنامه شاخص",
                                    Description = "گزارش شناسنامه شاخص",
                                    Icon = Icon.Folder.GetIconString(),
                                    Order = 10501,
                                    Key = "epm.report",
                                    ParentKey = "epm.report",
                                    Path = "/epm/report/indicator"
                                }, new()
                                {
                                    Title = "گزارش مقادیر شاخص‌ها",
                                    Description = "گزارش مقادیر شاخص‌ها",
                                    Icon = Icon.Folder.GetIconString(),
                                    Order = 10501,
                                    Key = "epm.report",
                                    ParentKey = "epm.report",
                                    Path = "/epm/report/indicatorvalue"
                                },
                            }
                        }
                    }
                }
            };
        }

        public static async Task SeedEpmsForNavigationAsync(
            IMenuPublicService menuPublicService,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("🚀 Starting Epm module seeding (Navigation)...");

            try
            {
                if (!ModuleHelper.IsActive(ModuleEnum.Epm))
                {
                    logger.LogInformation(
                        "Epm module is disabled; skipping navigation seed.");
                    return;
                }

                var menus = GetEpmMenuDefinitions();
                await menuPublicService.SyncModuleMenusAsync(menus, cancellationToken);
                logger.LogInformation("✅ Epm menus synced.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Epm navigation seeding");
                throw;
            }
        }


    }
}