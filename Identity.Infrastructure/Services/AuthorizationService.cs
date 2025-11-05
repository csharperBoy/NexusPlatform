using Core.Application.Abstractions;
using Core.Shared.Results;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Identity.Infrastructure.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IRepository<IdentityDbContext, IdentityUserRole<Guid>, Guid> _userRoleRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(
            IRepository<IdentityDbContext, IdentityUserRole<Guid>, Guid> userRoleRepository,
            RoleManager<ApplicationRole> roleManager,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            ILogger<AuthorizationService> logger)
        {
            _userRoleRepository = userRoleRepository;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> AssignRoleToUserAsync(Guid userId, string roleName)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null)
                    return Result.Fail("Role not found.");

                var userRole = new IdentityUserRole<Guid>
                {
                    UserId = userId,
                    RoleId = role.Id
                };

                await _userRoleRepository.AddAsync(userRole);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
                return Result.Ok();
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
        public async Task<Result> RemoveRoleFromUserAsync(Guid userId, string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return Result.Fail("Role not found.");

            var userRole = await _userRoleRepository.AsQueryable()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (userRole == null) return Result.Fail("User does not have this role.");

            await _userRoleRepository.DeleteAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
        {
            var roles = await GetUserRolesAsync(userId);
            return roles.Contains(roleName);
        }

        public async Task<IList<string>> GetUserRolesAsync(Guid userId)
        {
            var query = from ur in _userRoleRepository.AsQueryable()
                        join r in _roleManager.Roles on ur.RoleId equals r.Id
                        where ur.UserId == userId
                        select r.Name!;
            return await query.ToListAsync();
        }

        // Permissions (فعلاً پیاده‌سازی نشده)
        public Task<bool> UserHasPermissionAsync(Guid userId, string permission)
            => throw new NotImplementedException();

        public Task<Result> AssignPermissionToUserAsync(Guid userId, string permission)
            => throw new NotImplementedException();

    }

}
