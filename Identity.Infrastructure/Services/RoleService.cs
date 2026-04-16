using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Abstractions.Identity.PublicService;
using Core.Infrastructure.Repositories;
using Core.Shared.Results;
using Identity.Application.Commands.Role;
using Identity.Application.Commands.User;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries.Role;
using Identity.Application.Queries.User;
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
    public class RoleService : IRoleInternalService, IRolePublicService
    {
        private readonly IRepository<IdentityDbContext, IdentityUserRole<Guid>, Guid> _userRoleRepository;
        private readonly IRepository<IdentityDbContext, ApplicationRole, Guid> _roleRepository;
        private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;
        private readonly ILogger<RoleService> _logger;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ICachePublicService _cache;
        private readonly string baseCacheKey = "identity:role";
        //private readonly IRepository<IdentityDbContext, ApplicationRole, Guid> _roleRepository;
        //private readonly ISpecificationRepository<ApplicationRole, Guid> _roleSpecRepository;
        public RoleService(
            IRepository<IdentityDbContext, IdentityUserRole<Guid>, Guid> userRoleRepository,
            IRepository<IdentityDbContext, ApplicationRole, Guid> roleRepository,
            IUnitOfWork<IdentityDbContext> unitOfWork,
            ILogger<RoleService> logger,
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
             ICachePublicService cache
            //IRepository<IdentityDbContext, ApplicationRole, Guid> roleRepository,
            //ISpecificationRepository<ApplicationRole, Guid> roleSpecRepository,
            )
        {
            _userRoleRepository = userRoleRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _cache = cache;
            //_roleRepository = roleRepository;
            //_roleSpecRepository = roleSpecRepository;
        }
        #region crud actions

        public async Task DeleteRoleAsync(Guid Id)
        {
            var role = await _roleRepository.GetByIdAsync(Id);
            if (role == null) return;

            await _roleRepository.DeleteAsync(role);
            await _unitOfWork.SaveChangesAsync();
            await InvalidateRoleCachesAsync();
        }

        public async Task<Guid> CreateRoleAsync(CreateRoleCommand command)
        {
            var role = new ApplicationRole( command.Name , command.Description,command.OrderNum
               
            );
            var createRes = await _roleManager.CreateAsync(role);
            if (!createRes.Succeeded)
                throw new Exception(createRes.Errors.FirstOrDefault().Description);

            //await _unitOfWork.SaveChangesAsync();
            await InvalidateRoleCachesAsync();
            return role.Id;
        }
        public async Task<ApplicationRole?> GetById(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                throw new Exception($"role with name '{id}' not found.");
            }

            return role;
        }

        public async Task<IReadOnlyList<RoleDto>> getRoles(GetRolesQuery request)
        {
            var cacheKey = $"{baseCacheKey}:full";
            var cached = await _cache.GetAsync<IReadOnlyList<RoleDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for full resource tree");
                return cached;
            }
            var result = await _roleManager.Roles.Where(
                u => (request.Name != null ? u.Name.Contains(request.Name) : true)
                && (request.description != null ? u.Description.Contains(request.description) : true)
                ).Select(u => new RoleDto
                {
                    Id = u.Id,
                   Name = u.Name,
                   Description = u.Description,
                   OrderNum = u.OrderNum,
                }).AsNoTracking().ToListAsync();

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            return result;
        }

        public async Task UpdateRoleAsync(UpdateRoleCommand request)
        {
            var role = await _roleRepository.GetByIdAsync(request.Id);
            if (role == null) throw new ArgumentException("Role not found");
            if (role.ApplyChange(request.Name, request.Description, request.OrderNum))
            {
                await _roleManager.UpdateAsync(role);
                await InvalidateRoleCachesAsync();
            }
        }
        private async Task InvalidateRoleCachesAsync()
        {
            await _cache.RemoveByPatternAsync($"{baseCacheKey}:*");
        }
        

        #endregion
        public async Task<Guid> GetRoleId(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                throw new Exception($"Role with name '{roleName}' not found.");
            }

            return role.Id;
        }

        public async Task<Guid> GetAdminRoleIdAsync(CancellationToken cancellationToken = default)
        {
            return await GetRoleId("Admin");
        }

        public async Task<List<Guid>> GetAllUserRolesId(Guid userId)
        {
            List<Guid> RolesId = new List<Guid>();
            ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
            var rolesName = await _userManager.GetRolesAsync(user);
            foreach (var roleName in rolesName)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                RolesId.Add(role.Id);
            }
            return RolesId;
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

            var userRole = await (await _userRoleRepository.AsQueryable())
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (userRole == null) return Result.Fail("User does not have this role.");

            await _userRoleRepository.DeleteAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
        {
            var roles = await GetUserRolesAsync(userId);
            return roles.Any(r=>r.Name == roleName);
        }

        public async Task<IList<RoleDto>> GetUserRolesAsync(Guid userId)
        {
            var query = from ur in (await _userRoleRepository.AsQueryable())
                        join r in _roleManager.Roles on ur.RoleId equals r.Id
                        where ur.UserId == userId
                        select r;
            return await query.Select(r=>new RoleDto {
                                OrderNum = r.OrderNum,
                                Description = r.Description,
                                Name = r.Name,
                                Id = r.Id
                                }).ToListAsync();
        }

    }

}