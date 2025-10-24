using Authorization.Application.Interfaces;
using Core.Shared.Results;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            UserManager<ApplicationUser> userManager,
            ILogger<AuthorizationService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result> AssignRoleToUserAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result.Fail("User not found.");
                }

                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    return Result.Ok(); // یا می‌توانید خطا برگردانید
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
                    return Result.Ok();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Fail($"Failed to assign role: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleName} to user {UserId}", roleName, userId);
                return Result.Fail("An error occurred while assigning the role.");
            }
        }

        public async Task<Result> AssignDefaultRoleAsync(Guid userId)
        {
            const string defaultRole = "User";
            return await AssignRoleToUserAsync(userId, defaultRole);
        }

    }
}
