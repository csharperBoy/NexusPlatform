using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Specifications;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Core.Domain.Enums;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services
{
    /*
    public class DataScopeEvaluator : IDataScopeEvaluator
    {

        private readonly IRepository<DbContext, Permission, Guid> _permissionRepository;
        private readonly IRepository<DbContext, Resource, Guid> _resourceRepository;

        private readonly ISpecificationRepository<Permission, Guid> _permissionSpecRepository;
        private readonly ISpecificationRepository<Resource, Guid> _resourceSpecRepository;


        private readonly ILogger<DataScopeEvaluator> _logger;
        private readonly ICacheService _cache;

        public DataScopeEvaluator(
         IRepository<DbContext, Permission, Guid> permissionRepository,
         IRepository<DbContext, Resource, Guid> resourceRepository,
         ISpecificationRepository<Permission, Guid> permissionSpecRepository,
         ISpecificationRepository<Resource, Guid> resourceSpecRepository,
        ILogger<DataScopeEvaluator> logger,
            ICacheService cache)
        {
            _permissionRepository = permissionRepository;
            _resourceRepository = resourceRepository;
            _permissionSpecRepository = permissionSpecRepository;
            _resourceSpecRepository = resourceSpecRepository;
            _logger = logger;
            _cache = cache;
        }

        public Task<string> BuildDataFilterAsync(Guid userId, string resourceKey)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<DataScopeDto>> EvaluateAllDataScopesAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<DataScopeDto> EvaluateDataScopeAsync(Guid userId, string resourceKey)
        {
            throw new NotImplementedException();
        }

        public Task<ScopeType> EvaluateScopeAsync(Guid userId, string resourceKey, PermissionAction action)
        {
            throw new NotImplementedException();
        }
    }
*/

    public class DataScopeEvaluator : IDataScopeEvaluator
    {
        private readonly IRepository<DbContext, Permission, Guid> _permissionRepository;
        private readonly ISpecificationRepository<Permission, Guid> _permissionSpecRepository;
        private readonly ICurrentUserService _currentUserService; // برای دریافت اطلاعات پست و نقش فعلی
        private readonly ILogger<DataScopeEvaluator> _logger;

        public DataScopeEvaluator(
            IRepository<DbContext, Permission, Guid> permissionRepository,
            ISpecificationRepository<Permission, Guid> permissionSpecRepository, 
            ICurrentUserService currentUserService,
            ILogger<DataScopeEvaluator> logger)
        {
            _permissionSpecRepository = permissionSpecRepository;
            _permissionRepository = permissionRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// متد اصلی برای محاسبه اسکوپ یک اکشن خاص بر روی یک ریسورس
        /// </summary>
        public async Task<ScopeType> EvaluateScopeAsync(Guid userId, string resourceKey, PermissionAction action)
        {
            try
            {
                // 1. دریافت کانتکست امنیتی کاربر (پست سازمانی و نقش ها)
                // این اطلاعات معمولا در Claim ها هستند و توسط CurrentUserService ارائه می‌شوند
                var positionId = _currentUserService.PositionId;
                var roleIds = _currentUserService.RolesId ?? new List<Guid>();
                var personId = _currentUserService.PersonId;

                var spec = new EffectivePermissionsSpec(personId, positionId, roleIds.ToList(), resourceKey, action);
                var permissions = await _permissionSpecRepository.ListBySpecAsync(spec);

                if (permissions == null || !permissions.Any())
                {
                    _logger.LogDebug("No permissions found for user {UserId} on {Resource}:{Action}", userId, resourceKey, action);
                    return ScopeType.None;
                }

                // ۳. پیاده سازی سلسله مراتب اولویت ها (Priority Logic)

                // اولویت اول: شخص (Person) - اگر تنظیم مستقیمی برای خود فرد وجود داشته باشد
                var personPerm = permissions.FirstOrDefault(p => p.AssigneeType == AssigneeType.Person);
                if (personPerm != null)
                {
                    return personPerm.Type == PermissionType.Deny ? ScopeType.None : personPerm.Scope;
                }

                // اولویت دوم: منع دسترسی (Deny) - اگر در سطح نقش یا پست سازمانی Deny شده باشد
                if (permissions.Any(p => p.Type == PermissionType.Deny))
                {
                    return ScopeType.None;
                }

                // اولویت سوم: وسیع ترین دسترسی (Max Scope) بین نقش ها و پست های سازمانی
                // چون Enum ها مقدار عددی دارند (Self=1, Unit=2, All=99)، Max() درست عمل می کند
                var maxScope = permissions
                    .Where(p => p.Type == PermissionType.allow)
                    .Select(p => p.Scope)
                    .DefaultIfEmpty(ScopeType.None)
                    .Max();

                return maxScope;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating scope for user {UserId} on {Resource}:{Action}", userId, resourceKey, action);
                return ScopeType.None;
            }
        }

        // متدهای دیگر که فعلاً طبق خواسته شما به آن‌ها نیاز نداریم و پیاده‌سازی نمی‌شوند
        public Task<DataScopeDto> EvaluateDataScopeAsync(Guid userId, string resourceKey) => throw new NotImplementedException();
        public Task<IReadOnlyList<DataScopeDto>> EvaluateAllDataScopesAsync(Guid userId) => throw new NotImplementedException();
        public Task<string> BuildDataFilterAsync(Guid userId, string resourceKey) => throw new NotImplementedException();
    }
}