using Authorization.Application.DTOs;
using Authorization.Application.Interfaces;
using Authorization.Application.Security;
using Authorization.Domain.Entities;
using Core.Application.Abstractions;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IRepository<AuthorizationDbContext, Permission, Guid> _permissions;
        private readonly IRepository<AuthorizationDbContext, RolePermission, Guid>  _rolePermissions;
        private readonly IRepository<AuthorizationDbContext, UserPermission, Guid> _userPermissions;
        private readonly IRepository<AuthorizationDbContext, Resource, Guid> _resources;
        private readonly IMemoryCache _cache;

        private const string USER_CACHE_PREFIX = "perm_user_";
        private const string ROLE_CACHE_PREFIX = "perm_role_";

        public AuthorizationService(
             IRepository<AuthorizationDbContext, Permission, Guid> permissions,
             IRepository<AuthorizationDbContext, RolePermission, Guid> rolePermissions,
             IRepository<AuthorizationDbContext, UserPermission, Guid> userPermissions,
             IRepository<AuthorizationDbContext, Resource, Guid> resources,
             IMemoryCache cache
        {
            _permissions = permissions;
            _rolePermissions = rolePermissions;
            _userPermissions = userPermissions;
            _resources = resources;
            _cache = cache;
        }

        // ===================================================================
        // 1) Permission Registration (Old API) -------------------------------
        // ===================================================================

        public async Task RegisterPermissionsAsync(IEnumerable<PermissionDescriptor> descriptors, CancellationToken ct = default)
        {
            var all = descriptors.ToList();

            foreach (var d in all)
            {
                var exists = await _permissions
                    .AsQueryable()
                    .AnyAsync(p => p.Code == d.Code, ct);

                if (exists)
                    continue;

                // Ensure Resource exists
                var resource = await _resources.AsQueryable()
                    .FirstOrDefaultAsync(r => r.Code == d.ResourceCode, ct);

                if (resource == null)
                {
                    resource = new Resource(d.ResourceCode, d.ResourceName, d.ResourceType, d.ParentResourceCode);
                    await _resources.AddAsync(resource, ct);
                }

                var p = new Permission(resource.Id, d.PermissionCode, d.PermissionName, d.Description);
                await _permissions.AddAsync(p, ct);
            }
        }

        // ===================================================================
        // 2) Effective User Permissions (Old API + New Combined) -------------
        // ===================================================================

        public async Task<IEnumerable<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default)
        {
            return await _cache.GetOrCreateAsync($"{USER_CACHE_PREFIX}{userId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                // role permissions
                var rolePermCodes = await (
                    from rp in _rolePermissions.AsQueryable()
                    join p in _permissions.AsQueryable() on rp.PermissionId equals p.Id
                    select p.Code
                ).ToListAsync(ct);

                // user overrides
                var overrides = await (
                    from up in _userPermissions.AsQueryable()
                    join p in _permissions.AsQueryable() on up.PermissionId equals p.Id
                    where up.UserId == userId
                    select new { p.Code, up.Granted }
                ).ToListAsync(ct);

                var set = new HashSet<string>(rolePermCodes);

                foreach (var ov in overrides)
                {
                    if (ov.Granted)
                        set.Add(ov.Code);
                    else
                        set.Remove(ov.Code);
                }

                return set.AsEnumerable();
            });
        }

        // ===================================================================
        // 3) UserHasPermissionAsync (Old API) -------------------------------
        // ===================================================================

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
        {
            var effective = await GetEffectivePermissionsAsync(userId, ct);
            return effective.Contains(permissionCode);
        }

        // ===================================================================
        // 4) Cache Invalidate (Old API) -------------------------------------
        // ===================================================================

        public Task InvalidateUserCacheAsync(Guid userId)
        {
            _cache.Remove($"{USER_CACHE_PREFIX}{userId}");
            return Task.CompletedTask;
        }

        public Task InvalidateRoleCacheAsync(Guid roleId)
        {
            _cache.Remove($"{ROLE_CACHE_PREFIX}{roleId}");
            return Task.CompletedTask;
        }

        // ===================================================================
        // 5) Permission DTO List (New API) ----------------------------------
        // ===================================================================

        public async Task<List<PermissionDto>> GetAllAsync(CancellationToken ct)
        {
            var query =
                from p in _permissions.AsQueryable()
                join r in _resources.AsQueryable() on p.ResourceId equals r.Id
                select new PermissionDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    ResourceCode = r.Code,
                    Description = p.Description
                };

            return await query.ToListAsync(ct);
        }

        // ===================================================================
        // 6) Role Permission IDs (New API) ----------------------------------
        // ===================================================================

        public async Task<List<Guid>> GetRolePermissionIdsAsync(Guid roleId, CancellationToken ct)
        {
            return await _rolePermissions
                .AsQueryable()
                .Where(x => x.RoleId == roleId)
                .Select(x => x.PermissionId)
                .ToListAsync(ct);
        }

        // ===================================================================
        // 7) Update Role Permissions (New API) -------------------------------
        // ===================================================================

        public async Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds, CancellationToken ct)
        {
            var old = await _rolePermissions
                .AsQueryable()
                .Where(x => x.RoleId == roleId)
                .ToListAsync(ct);

            foreach (var o in old)
                await _rolePermissions.DeleteAsync(o.Id, ct);

            foreach (var pid in permissionIds)
            {
                var rp = new RolePermission(roleId, pid);
                await _rolePermissions.AddAsync(rp, ct);
            }

            await InvalidateRoleCacheAsync(roleId);
        }

        // ===================================================================
        // 8) User Overrides (New API) ----------------------------------------
        // ===================================================================

        public async Task AddUserOverrideAsync(Guid userId, Guid permissionId, bool granted, string? scope, CancellationToken ct)
        {
            var entity = new UserPermission(userId, permissionId, granted, scope);
            await _userPermissions.AddAsync(entity, ct);
            await InvalidateUserCacheAsync(userId);
        }

        public async Task RemoveUserOverrideAsync(Guid userId, Guid overrideId, CancellationToken ct)
        {
            await _userPermissions.DeleteAsync(overrideId, ct);
            await InvalidateUserCacheAsync(userId);
        }
    }
}
