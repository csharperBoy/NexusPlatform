using Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;


namespace Identity.Infrastructure.Data
{
    public static class IdentitySeedData
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            var adminEmail = config["Seed:AdminEmail"] ?? "admin@local";
            var adminPassword = config["Seed:AdminPassword"] ?? "ChangeMe123!";
            var adminUserName = config["Seed:AdminUserName"] ?? "admin";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser(Guid.NewGuid(), adminUserName, adminEmail)
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

    }
}