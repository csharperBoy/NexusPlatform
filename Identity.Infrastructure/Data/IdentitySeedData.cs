using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Identity.Infrastructure.Data
{
    public static class IdentitySeedData
    {
        public static async Task StartSeedAsync(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IResourcePublicService resourcePublicService,
            IPermissionPublicService permissionPublicService,
            IRolePublicService roleService,
            ILogger logger,
            IConfiguration config)
        {
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager, config); 
            await AssignAdminRoleToAdminUserAsync(userManager, config);
            await SeedIdentotiesForAuthorizationAsync(resourcePublicService,permissionPublicService,roleService,logger);
        }
        public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    (
                         roleName,
                         $"{roleName} role"
                    );

                    var result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role {roleName}: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }

        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            var adminEmail = config["Seed:AdminEmail"] ?? "admin@local";
            var adminPassword = config["Seed:AdminPassword"] ?? "ChangeMe123!";
            var adminUserName = config["Seed:AdminUserName"] ?? "admin";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser(Guid.Empty, adminUserName, adminEmail)
                {
                    EmailConfirmed = true
                };
                adminUser.NickName = "System Administrator";
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create default admin user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            var initializerUser = await userManager.FindByEmailAsync("intitializer@info.com");
            if (initializerUser == null)
            {
                initializerUser = new ApplicationUser(Guid.Empty, "intitializer", "intitializer@info.com")
                {
                    EmailConfirmed = true
                };

                adminUser.NickName = "System intitializer";
                var result = await userManager.CreateAsync(initializerUser, "Init@123456789");
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create default initializer user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
        public static async Task AssignAdminRoleToAdminUserAsync(
                                     UserManager<ApplicationUser> userManager,
                                     IConfiguration config)
        {
            var adminEmail = config["Seed:AdminEmail"] ?? "admin@local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                throw new Exception("Admin user not found, cannot assign role.");
            }

            // بررسی می‌کند آیا کاربر در حال حاضر نقش Admin را دارد یا خیر
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var result = await userManager.AddToRoleAsync(adminUser, "Admin");
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to assign Admin role to admin user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        #region seed resource and permission
        // تعریف ساختار درختی منابع ماژول Audit
        private static List<ResourceDto> GetIdentityResourceDefinitions()
        {
            return new List<ResourceDto>
            {
                new()
                {
                    Key = "identity",
                    Name = "identity",
                    Type =ResourceType.Module,
                    Category = ResourceCategory.System,
                    Description = "Identity management module",
                    DisplayOrder = 3000,
                    Icon = "shield",
                    //Path = "/audit",
                    Children = new List<ResourceDto>
                    {
                        new()
                        {
                            Key = "identity.user",
                            Name = "identity Users",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Identity Users management",
                            DisplayOrder = 3001,
                            Icon = "list",
                            //Path = "/audit/logs"
                        }, new()
                        {
                            Key = "identity.role",
                            Name = "identity Roles",
                            Type =ResourceType.Data,
                            Category =ResourceCategory.System,
                            Description = "Identity Roles management",
                            DisplayOrder = 3002,
                            Icon = "list",
                            //Path = "/audit/logs"
                        }
                    }
                }
            };
        }

        // تعریف پرمیشن‌های پیش‌فرض ماژول Audit
        private static List<PermissionDto> GetIdentityPermissionDefinitions(Guid roleId)
        {
            return new List<PermissionDto>
            {
                new()
                {
                    ResourceKey = "identity.user",
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

                    Description = "Full access to Identity Users"
                },new()
                {
                    ResourceKey = "identity.role",
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

                    Description = "Full access to Identity Roles"
                }
            };
        }
        public static async Task SeedIdentotiesForAuthorizationAsync(
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
                var resources = GetIdentityResourceDefinitions();
                await resourcePublicService.SyncModuleResourcesAsync(resources, cancellationToken);
                logger.LogInformation("✅ Identity resources synced successfully.");

                // 2. ثبت پرمیشن‌ها (Permissions)
                // ابتدا آیدی نقش ادمین را از سرویس Identity می‌گیریم
                var adminRoleId = await roleService.GetAdminRoleIdAsync(cancellationToken);

                var permissions = GetIdentityPermissionDefinitions(adminRoleId);
                await permissionPublicService.SeedRolePermissionsAsync(permissions, cancellationToken);
                logger.LogInformation("✅ Identity permissions seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during Identity module seeding");
                throw;
            }
        }
        #endregion

    }
}