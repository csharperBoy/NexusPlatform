using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;


namespace Identity.Infrastructure.Data
{
    public static class IdentitySeedData
    {
        public static async Task StartSeedAsync(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager, config); 
            await AssignAdminRoleToAdminUserAsync(userManager, config);
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
                adminUser.SetFullName("System", "Administrator");
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
                adminUser.SetFullName("System", "intitializer");
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

    }
}