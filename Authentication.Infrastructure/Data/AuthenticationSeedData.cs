using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;


namespace Authentication.Infrastructure.Data
{
    public static class AuthenticationSeedData
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
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create default admin user: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}