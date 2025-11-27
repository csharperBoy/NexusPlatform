using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Specifications;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services
{
    public class DataScopeEvaluator : IDataScopeEvaluator
    {
        private readonly ISpecificationRepository<DataScope, Guid> _dataScopeSpecRepository;
        private readonly ILogger<DataScopeEvaluator> _logger;
        private readonly ICacheService _cache;

        public DataScopeEvaluator(
            ISpecificationRepository<DataScope, Guid> dataScopeSpecRepository,
            ILogger<DataScopeEvaluator> logger,
            ICacheService cache)
        {
            _dataScopeSpecRepository = dataScopeSpecRepository;
            _logger = logger;
            _cache = cache;
        }

        public async Task<DataScopeDto> EvaluateDataScopeAsync(Guid userId, string resourceKey)
        {
            var cacheKey = $"auth:datascope:{userId}:{resourceKey}";

            try
            {
                // بررسی کش
                var cached = await _cache.GetAsync<DataScopeDto>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for data scope: {Key}", cacheKey);
                    return cached;
                }

                // دریافت تمام محدوده‌های داده فعال
                var effectiveDataScopesSpec = new EffectiveDataScopesSpec();
                var allDataScopes = await _dataScopeSpecRepository.ListBySpecAsync(effectiveDataScopesSpec);

                // فیلتر محدوده‌های مربوط به کاربر و منبع
                var userDataScopes = allDataScopes
                    .Where(ds => ds.AppliesTo(AssigneeType.Person, userId) &&
                                ds.Resource.Key == resourceKey)
                    .ToList();

                // محاسبه محدوده مؤثر
                var effectiveDataScope = CalculateEffectiveDataScope(userDataScopes, resourceKey);

                // ذخیره در کش
                await _cache.SetAsync(cacheKey, effectiveDataScope, TimeSpan.FromMinutes(10));

                _logger.LogDebug(
                    "Evaluated data scope for user {UserId} to resource {Resource}: {ScopeType}",
                    userId, resourceKey, effectiveDataScope.Scope);

                return effectiveDataScope;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error evaluating data scope for user {UserId} to resource {Resource}",
                    userId, resourceKey);
                throw;
            }
        }

        public async Task<IReadOnlyList<DataScopeDto>> EvaluateAllDataScopesAsync(Guid userId)
        {
            var cacheKey = $"auth:alldatascopes:{userId}";

            try
            {
                // بررسی کش
                var cached = await _cache.GetAsync<IReadOnlyList<DataScopeDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for all data scopes: {Key}", cacheKey);
                    return cached;
                }

                // دریافت تمام محدوده‌های داده فعال
                var effectiveDataScopesSpec = new EffectiveDataScopesSpec();
                var allDataScopes = await _dataScopeSpecRepository.ListBySpecAsync(effectiveDataScopesSpec);

                var userDataScopes = allDataScopes
                    .Where(ds => ds.AppliesTo(AssigneeType.Person, userId))
                    .GroupBy(ds => ds.Resource.Key)
                    .Select(g => CalculateEffectiveDataScope(g.ToList(), g.Key))
                    .ToList();

                // ذخیره در کش
                await _cache.SetAsync(cacheKey, userDataScopes, TimeSpan.FromMinutes(5));

                _logger.LogInformation(
                    "Evaluated {Count} data scopes for user {UserId}",
                    userDataScopes.Count, userId);

                return userDataScopes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating all data scopes for user {UserId}", userId);
                throw;
            }
        }

        public async Task<string> BuildDataFilterAsync(Guid userId, string resourceKey)
        {
            try
            {
                var dataScope = await EvaluateDataScopeAsync(userId, resourceKey);
                var filter = BuildFilterCondition(dataScope);

                _logger.LogDebug(
                    "Built data filter for user {UserId} to resource {Resource}: {Filter}",
                    userId, resourceKey, filter);

                return filter;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error building data filter for user {UserId} to resource {Resource}",
                    userId, resourceKey);
                throw;
            }
        }

        private DataScopeDto CalculateEffectiveDataScope(List<DataScope> dataScopes, string resourceKey)
        {
            if (!dataScopes.Any())
            {
                return new DataScopeDto
                {
                    ResourceKey = resourceKey,
                    Scope = ScopeType.Self, // پیش‌فرض: فقط داده‌های خود کاربر
                    Depth = 1,
                    CustomFilter = string.Empty,
                    EvaluatedAt = DateTime.UtcNow
                };
            }

            // اولویت: SpecificUnit > Unit > Subtree > Self > All
            var effectiveDataScope = dataScopes
                .OrderByDescending(ds => ds.Scope) // بر اساس مقادیر enum
                .ThenByDescending(ds => ds.Depth)
                .First();

            return new DataScopeDto
            {
                Id = effectiveDataScope.Id,
                ResourceId = effectiveDataScope.ResourceId,
                ResourceKey = resourceKey,
                AssigneeType = effectiveDataScope.AssigneeType,
                AssigneeId = effectiveDataScope.AssigneeId,
                Scope = effectiveDataScope.Scope,
                SpecificUnitId = effectiveDataScope.SpecificUnitId,
                CustomFilter = effectiveDataScope.CustomFilter,
                Depth = effectiveDataScope.Depth,
                IsActive = effectiveDataScope.IsActive,
                EffectiveFrom = effectiveDataScope.EffectiveFrom,
                ExpiresAt = effectiveDataScope.ExpiresAt,
                Description = effectiveDataScope.Description,
                CreatedAt = effectiveDataScope.CreatedAt,
                CreatedBy = effectiveDataScope.CreatedBy,
                EvaluatedAt = DateTime.UtcNow,
                DataScopeCount = dataScopes.Count
            };
        }

        private string BuildFilterCondition(DataScopeDto dataScope)
        {
            return dataScope.Scope switch
            {
                ScopeType.All => "1=1", // دسترسی به تمام داده‌ها
                ScopeType.Self => $"UserId = '{dataScope.AssigneeId}'", // فقط داده‌های خود کاربر
                ScopeType.Unit => $"UnitId IN (SELECT UnitId FROM UserUnits WHERE UserId = '{dataScope.AssigneeId}')",
                ScopeType.SpecificUnit => $"UnitId = '{dataScope.SpecificUnitId}'",
                ScopeType.Subtree => BuildSubtreeFilter(dataScope),
                _ => "1=0" // پیش‌فرض: هیچ دسترسی
            };
        }

        private string BuildSubtreeFilter(DataScopeDto dataScope)
        {
            // ساخت شرط برای دسترسی سلسله مراتبی
            return $@"UnitId IN (
                SELECT UnitId FROM OrganizationalUnits 
                WHERE Path LIKE '%/{dataScope.AssigneeId}/%' 
                AND Level <= {dataScope.Depth}
            )";
        }
    }
}