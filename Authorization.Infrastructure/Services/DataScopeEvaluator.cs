using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Specifications;
using Authorization.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Core.Domain.Enums;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services
{

    public class DataScopeEvaluator : IDataScopeEvaluator
    {
        // استفاده از ریپازیتوری اسپسیفیکیشن برای کوئری‌های بهینه
        private readonly ISpecificationRepository<Permission, Guid> _permissionSpecRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DataScopeEvaluator> _logger;

        public DataScopeEvaluator(
            ISpecificationRepository<Permission, Guid> permissionSpecRepository,
            ICurrentUserService currentUserService,
            ILogger<DataScopeEvaluator> logger)
        {
            _permissionSpecRepository = permissionSpecRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        // ----------------------------------------------------------------
        // 1. محاسبه اسکوپ برای یک اکشن خاص (مثلا Edit)
        // ----------------------------------------------------------------
        public async Task<ScopeType> EvaluateScopeAsync(/*Guid userId,*/ string resourceKey, PermissionAction action)
        {
            try
            {
                var (personId, positionId, roleIds) = GetUserContext();

                // استفاده از اسپک فیلتر شده (Specific Resource & Action)
                var spec = new EffectivePermissionsSpec(personId, positionId, roleIds, resourceKey, action);
                var permissions = await _permissionSpecRepository.ListBySpecAsync(spec);

                return CalculateScopeFromList(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating scope for {Resource}:{Action}", resourceKey, action);
                return ScopeType.None;
            }
        }

        // ----------------------------------------------------------------
        // 2. محاسبه اسکوپ داده (پیش‌فرض روی View) برای یک ریسورس
        // ----------------------------------------------------------------
        public async Task<DataScopeDto> EvaluateDataScopeAsync(/*Guid userId,*/ string resourceKey)
        {
            // وقتی صحبت از Data Scope است، یعنی کاربر چه چیزی را می‌تواند ببیند (View)
            var scope = await EvaluateScopeAsync( resourceKey, PermissionAction.View);

            return new DataScopeDto
            {
                ResourceKey = resourceKey,
                Scope = scope
            };
        }

        // ----------------------------------------------------------------
        // 3. محاسبه تمام اسکوپ‌های کاربر (برای کش کردن یا استفاده در GetScopeForUser)
        // ----------------------------------------------------------------
        public async Task<IReadOnlyList<DataScopeDto>> EvaluateAllDataScopesAsync()
        {
            try
            {
                var (personId, positionId, roleIds) = GetUserContext();

                // دریافت کل پرمیشن‌های کاربر (بدون فیلتر ریسورس)
                var spec = new UserPermissionsSpec(personId, positionId, roleIds);
                var allPermissions = await _permissionSpecRepository.ListBySpecAsync(spec);

                // گروه‌بندی بر اساس ریسورس
                // هدف: برای هر ریسورس، اسکوپ نهایی View را محاسبه کنیم
                var resourceGroups = allPermissions
                    .GroupBy(p => p.Resource.Key)
                    .Select(g => new DataScopeDto
                    {
                        ResourceKey = g.Key,
                        // فقط پرمیشن‌های View را برای اسکوپ داده لحاظ می‌کنیم
                        Scope = CalculateScopeFromList(g.Where(p => p.Action == PermissionAction.View).ToList())
                    })
                    .ToList();

                return resourceGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating all data scopes for user {UserId}", _currentUserService.PersonId);
                return new List<DataScopeDto>();
            }
        }

        // ----------------------------------------------------------------
        // 4. ساخت فیلتر SQL (اختیاری - اگر برای Dapper یا Raw SQL نیاز دارید)
        // ----------------------------------------------------------------
        public async Task<string> BuildDataFilterAsync( string resourceKey)
        {
            var scope = await EvaluateScopeAsync( resourceKey, PermissionAction.View);

            // بازگشت شرط SQL بر اساس اسکوپ
            return scope switch
            {
                ScopeType.All => "1=1", // همه چیز
                ScopeType.None => "1=0", // هیچ چیز
                ScopeType.Self => $"OwnerPersonId = '{_currentUserService.PersonId}'",
                ScopeType.Unit => $"OwnerOrganizationUnitId = '{_currentUserService.OrganizationUnitId}'",
                // نکته: برای UnitAndBelow نیاز به لاجیک پیچیده‌تر SQL (Like) است
                ScopeType.UnitAndBelow => $"OwnerOrganizationUnitId IN (SELECT Id FROM OrganizationUnits WHERE Path LIKE ...)",
                _ => "1=0"
            };
        }

        // ================================================================
        // Private Helpers
        // ================================================================

        /// <summary>
        /// منطق مرکزی اولویت‌بندی (Priority Logic)
        /// </summary>
        private ScopeType CalculateScopeFromList(IEnumerable<Permission> permissions)
        {
            if (permissions == null || !permissions.Any()) return ScopeType.None;

            // اولویت ۱: شخص
            var personPerm = permissions.FirstOrDefault(p => p.AssigneeType == AssigneeType.Person);
            if (personPerm != null)
            {
                return personPerm.Type == PermissionType.Deny ? ScopeType.None : personPerm.Scope;
            }

            // اولویت ۲: وجود Deny در نقش/پست
            if (permissions.Any(p => p.Type == PermissionType.Deny))
            {
                return ScopeType.None;
            }

            // اولویت ۳: Max Scope
            return permissions
                .Where(p => p.Type == PermissionType.allow)
                .Select(p => p.Scope)
                .DefaultIfEmpty(ScopeType.None)
                .Max();
        }

        private (Guid PersonId, Guid? PositionId, List<Guid> RoleIds) GetUserContext()
        {
            var posId = _currentUserService.PositionId;
            var roleIds = _currentUserService.RolesId ?? new List<Guid>(); // اصلاح نام پراپرتی بر اساس ICurrentUserService شما
            var personId = _currentUserService.PersonId;
            return (personId??Guid.Empty, posId, roleIds.ToList());
        }
    }
}