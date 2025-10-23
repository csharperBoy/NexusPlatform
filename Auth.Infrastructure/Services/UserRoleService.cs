// Auth.Infrastructure/Services/UserRoleService.cs
using Auth.Application.Interfaces;
using Auth.Infrastructure.Identity;
using Core.Shared.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserRoleService> _logger;

        public UserRoleService(
            UserManager<ApplicationUser> userManager,
            ILogger<UserRoleService> logger)
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
                    _logger.LogWarning("User {UserId} not found for role assignment", userId);
                    return Result.Fail("User not found.");
                }

                // بررسی آیا کاربر از قبل این نقش را دارد
                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    _logger.LogInformation("User {UserId} already has role {RoleName}", userId, roleName);
                    return Result.Ok(); // یا می‌توانید خطا برگردانید
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully assigned role {RoleName} to user {UserId}", roleName, userId);
                    return Result.Ok();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to assign role {RoleName} to user {UserId}: {Errors}", roleName, userId, errors);
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
            // می‌توانید نقش پیش‌فرض را از تنظیمات بخوانید
            const string defaultRole = "User";
            return await AssignRoleToUserAsync(userId, defaultRole);
        }

        public async Task<Result> RemoveRoleFromUserAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Result.Fail("User not found.");
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully removed role {RoleName} from user {UserId}", roleName, userId);
                    return Result.Ok();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Result.Fail($"Failed to remove role: {errors}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role {RoleName} from user {UserId}", roleName, userId);
                return Result.Fail("An error occurred while removing the role.");
            }
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return false;

                return await _userManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role {RoleName} for user {UserId}", roleName, userId);
                return false;
            }
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) return new List<string>();

                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
                return new List<string>();
            }
        }
    }
}