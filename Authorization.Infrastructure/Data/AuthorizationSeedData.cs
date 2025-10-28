using Authorization.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Data
{
    public static class AuthorizationSeedData
    {
        public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"{roleName} role",
                        CreatedAt = DateTime.UtcNow
                    };

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