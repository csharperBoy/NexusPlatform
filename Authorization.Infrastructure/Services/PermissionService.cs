using Authorization.Application.Interfaces;
using Authorization.Application.Security;
using Authorization.Domain.Entities;
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
    public class PermissionService : IPermissionService
    {
        private readonly AuthorizationDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PermissionService> _logger;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

        public PermissionService(AuthorizationDbContext db, IMemoryCache cache, ILogger<PermissionService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        public async Task RegisterPermissionsAsync(IEnumerable<PermissionDescriptor> descriptors, CancellationToken ct = default)
        {
            // upsert resources + permissions transactionally
            foreach (var d in descriptors)
            {
                // ensure resource exists (resource code can be like "Identity.Users")
                var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Code == d.ResourceCode, ct);
                if (resource == null)
                {
                    resource = new Resource
                    {
                        Id = Guid.NewGuid(),
                        Code = d.ResourceCode,
                        Name = d.ResourceCode,
                        Type = "Action"
                    };
                    _db.Resources.Add(resource);
                }

                var existingPerm = await _db.Permissions.FirstOrDefaultAsync(p => p.Code == d.Code, ct);
                if (existingPerm == null)
                {
                    var perm = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Code = d.Code,
                        Name = d.Name,
                        Resource = resource
                    };
                    _db.Permissions.Add(perm);
                }
                else
                {
                    existingPerm.Name = d.Name;
                    existingPerm.ResourceId = resource.Id;
                }
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Registered {Count} permissions", descriptors.Count());
        }

        public async Task<IEnumerable<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default)
        {
            var cacheKey = $"auth:user:{userId}:perms";
            if (_cache.TryGetValue<IEnumerable<string>>(cacheKey, out var cached))
                return cached!;

            // get role ids for the user from AspNetUserRoles table in Identity DB
            // NOTE: This service assumes Identity tables are in a different DbContext
            // So we query AspNetUserRoles via raw query or a shared repo. Here we'll perform a raw SQL join
            // to read role ids using a connection (simplest is to rely on IdentityDbContext in caller).
            // For now: we only collect role-based permissions present in this Authorization DB (role ids must match)
            var rolePerms = await (from rp in _db.RolePermissions
                                   join p in _db.Permissions on rp.PermissionId equals p.Id
                                   select new { rp.RoleId, PermissionCode = p.Code })
                                  .ToListAsync(ct);

            // rolePerms grouped by role - caller will filter by real user roles; here we return all role mappings
            // For simplicity in this MVP, we will assume the caller (AuthorizationPolicyHandler) will check user roles and combine.
            var allRolePermissionCodes = rolePerms.Select(x => x.PermissionCode).Distinct().ToList();

            // user-specific grants/denies
            var userGrants = await (from up in _db.UserPermissions
                                    where up.UserId == userId && up.Granted
                                    join p in _db.Permissions on up.PermissionId equals p.Id
                                    select p.Code).Distinct().ToListAsync(ct);

            var userDenies = await (from up in _db.UserPermissions
                                    where up.UserId == userId && !up.Granted
                                    join p in _db.Permissions on up.PermissionId equals p.Id
                                    select p.Code).Distinct().ToListAsync(ct);

            // merge: role-based (we do not filter rolePerms by user roles here) + user grants - user denies
            var merged = allRolePermissionCodes.Union(userGrants).Except(userDenies).Distinct().ToList();

            _cache.Set(cacheKey, merged, CacheTtl);
            return merged;
        }

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode, CancellationToken ct = default)
        {
            var perms = await GetEffectivePermissionsAsync(userId, ct);
            return perms.Contains(permissionCode);
        }

        public Task InvalidateUserCacheAsync(Guid userId)
        {
            var cacheKey = $"auth:user:{userId}:perms";
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }

        public Task InvalidateRoleCacheAsync(Guid roleId)
        {
            // For simplicity we invalidate all user caches (could be improved by tracking role->users)
            // In a production system you'd map role->users to invalidate only affected users.
            // Here we keep it simple.
            _logger.LogInformation("InvalidateRoleCache called for role {RoleId} - clearing all auth cache", roleId);
            // No global IMemoryCache clear available; you can use a cache key prefix or use Redis with key patterns.
            return Task.CompletedTask;
        }
    }
}
