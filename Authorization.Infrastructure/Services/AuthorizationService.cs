using Authorization.Application.DTOs;
using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.DTOs.Permissions;
using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی همزمان IAuthorizationService (کامل) و IAuthorizationChecker (سبک)
    /// </summary>
    public class AuthorizationService : IAuthorizationService, IAuthorizationChecker
    {
        private readonly IPermissionEvaluator _permissionEvaluator;
        private readonly IDataScopeEvaluator _dataScopeEvaluator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthorizationService> _logger;
        private readonly ICacheService _cache;

        public AuthorizationService(
            IPermissionEvaluator permissionEvaluator,
            IDataScopeEvaluator dataScopeEvaluator,
            ICurrentUserService currentUserService,
            ILogger<AuthorizationService> logger,
            ICacheService cache)
        {
            _permissionEvaluator = permissionEvaluator;
            _dataScopeEvaluator = dataScopeEvaluator;
            _currentUserService = currentUserService;
            _logger = logger;
            _cache = cache;
        }
        public async Task<ScopeType> GetPermissionScopeAsync(Guid userId, string resourceKey, PermissionAction action)
        {
            // کلید کش شامل اکشن هم می‌شود
            var cacheKey = $"auth:scope:{userId}:{resourceKey}:{action}";

            try
            {
                // 1. بررسی کش
                var cached = await _cache.GetAsync<ScopeType?>(cacheKey);
                if (cached.HasValue)
                {
                    _logger.LogDebug("Cache hit for scope check: {Key} -> {Scope}", cacheKey, cached.Value);
                    return cached.Value;
                }

                // 2. واگذاری محاسبه به Evaluator
                // لاجیک پیچیده سلسله مراتب در اینجا صدا زده می‌شود
                var scope = await _dataScopeEvaluator.EvaluateScopeAsync( resourceKey, action);

                // 3. ذخیره در کش
                await _cache.SetAsync(cacheKey, scope, TimeSpan.FromMinutes(10));

                _logger.LogInformation(
                    "Scope calculated for user  on {Resource}:{Action} = {Scope}",
                     resourceKey, action, scope);

                return scope;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating scope for user {UserId}", _currentUserService.PersonId);
                return ScopeType.None; // Fail Secure
            }
        }

        // ================================================
        // پیاده‌سازی IAuthorizationChecker (برای Core و ماژول‌های دیگر)
        // ================================================

        public async Task<bool> CheckAccessAsync(Guid userId, string resourceKey, string action)
        {
            var cacheKey = $"auth:access:{userId}:{resourceKey}:{action}";

            try
            {
                // بررسی کش
                var cached = await _cache.GetAsync<bool?>(cacheKey);
                if (cached.HasValue)
                {
                    _logger.LogDebug("Cache hit for access check: {Key}", cacheKey);
                    return cached.Value;
                }

                // ارزیابی دسترسی
                var hasAccess = await _permissionEvaluator.HasPermissionAsync(userId, resourceKey, action);

                // ذخیره در کش
                await _cache.SetAsync(cacheKey, hasAccess, TimeSpan.FromMinutes(10));

                _logger.LogInformation(
                    "Access check for user {UserId} to {Resource}:{Action} = {Result}",
                    userId, resourceKey, action, hasAccess ? "GRANTED" : "DENIED");

                return hasAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in access check for user {UserId} to {Resource}:{Action}",
                    userId, resourceKey, action);
                return false; // Fail secure
            }
        }

        public async Task<bool> CheckAccessAsync(string resourceKey, string action)
        {
            var userId = _currentUserService.UserId;
            if (userId == null || userId == Guid.Empty)
            {
                _logger.LogWarning("No current user found when checking access to {Resource}:{Action}",
                    resourceKey, action);
                return false;
            }

            return await CheckAccessAsync(userId.Value, resourceKey, action);
        }

        // ================================================
        // پیاده‌سازی IAuthorizationService (کامل)
        // ================================================

        public async Task<AccessResult> CheckAccessAsync(AccessRequest request)
        {
            try
            {
                var hasAccess = await CheckAccessAsync(request.UserId, request.ResourceKey, request.Action);

                if (hasAccess)
                {
                    _logger.LogDebug(
                        "Advanced access check GRANTED for user {UserId} to {Resource}:{Action}",
                        request.UserId, request.ResourceKey, request.Action);
                    return AccessResult.Grant();
                }

                // بررسی جزئیات دلیل رد دسترسی
                var permissions = await _permissionEvaluator.EvaluateUserPermissionsAsync(request.UserId, request.ResourceKey);
                var dataScopes = await _dataScopeEvaluator.EvaluateDataScopeAsync( request.ResourceKey);

                var denyReason = "User does not have required permissions";
                var details = new Dictionary<string, object>
                {
                    ["UserId"] = request.UserId,
                    ["ResourceKey"] = request.ResourceKey,
                    ["Action"] = request.Action,
                    ["HasPermissions"] = permissions != null,
                    ["HasDataScopes"] = dataScopes != null,
                    ["CheckedAt"] = DateTime.UtcNow
                };

                _logger.LogWarning(
                    "Advanced access check DENIED for user {UserId} to {Resource}:{Action}. Reason: {Reason}",
                    request.UserId, request.ResourceKey, request.Action, denyReason);

                return AccessResult.Deny(denyReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error in advanced access check for user {UserId} to {Resource}:{Action}",
                    request.UserId, request.ResourceKey, request.Action);
                return AccessResult.Deny("Authorization system error");
            }
        }

        public async Task<UserAccessDto> GetUserEffectiveAccessAsync(Guid userId)
        {
            var cacheKey = $"auth:useraccess:{userId}";

            try
            {
                // بررسی کش
                var cached = await _cache.GetAsync<UserAccessDto>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for user access: {Key}", cacheKey);
                    return cached;
                }

                // محاسبه دسترسی‌های مؤثر
                var permissions = await _permissionEvaluator.EvaluateAllUserPermissionsAsync(userId);
                var dataScopes = await _dataScopeEvaluator.EvaluateAllDataScopesAsync();

                var userAccess = new UserAccessDto
                {
                    UserId = userId,
                    Permissions = permissions?.ToList() ?? new List<EffectivePermissionDto>(),
                    DataScopes = dataScopes?.ToList() ?? new List<DataScopeDto>(),
                };

                // ذخیره در کش
                await _cache.SetAsync(cacheKey, userAccess, TimeSpan.FromMinutes(5));

                _logger.LogInformation(
                    "Generated effective access for user {UserId} with {PermissionCount} permissions and {DataScopeCount} data scopes",
                    userId, userAccess.Permissions.Count, userAccess.DataScopes.Count);

                return userAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating effective access for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CheckMultipleAccessAsync(Guid userId, IEnumerable<(string Resource, string Action)> permissions)
        {
            try
            {
                foreach (var (resource, action) in permissions)
                {
                    var hasAccess = await CheckAccessAsync(userId, resource, action);
                    if (!hasAccess)
                    {
                        _logger.LogWarning(
                            "Multiple access check FAILED: User {UserId} missing access to {Resource}:{Action}",
                            userId, resource, action);
                        return false;
                    }
                }

                _logger.LogInformation(
                    "Multiple access check PASSED: User {UserId} has all {PermissionCount} required permissions",
                    userId, permissions.Count());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in multiple access check for user {UserId}", userId);
                return false;
            }
        }

        public async Task<List<ScopeType>> GetScopeForUser(Guid userId, string resourceKey)
        {
            try
            {
                var dataScopes = await _dataScopeEvaluator.EvaluateAllDataScopesAsync();
                return dataScopes.Select(d => d.Scope).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}