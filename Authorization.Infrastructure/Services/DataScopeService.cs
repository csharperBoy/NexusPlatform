using Authorization.Application.Commands.DataScopes;
using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Authorization.Domain.Specifications;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Authorization.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services
{
  /*  public class DataScopeService : IDataScopeService
    {
        private readonly IRepository<AuthorizationDbContext, Resource, Guid> _resourceRepository;
        private readonly IUnitOfWork<AuthorizationDbContext> _unitOfWork;
        private readonly ILogger<DataScopeService> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly ICacheService _cache;
        private readonly IDataScopeEvaluator _dataScopeEvaluator;

        public DataScopeService(
            IRepository<AuthorizationDbContext, Resource, Guid> resourceRepository,
            IUnitOfWork<AuthorizationDbContext> unitOfWork,
            ILogger<DataScopeService> logger,
            ICurrentUserService currentUser,
            ICacheService cache,
            IDataScopeEvaluator dataScopeEvaluator) // اضافه شد
        {
            _resourceRepository = resourceRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUser = currentUser;
            _cache = cache;
            _dataScopeEvaluator = dataScopeEvaluator;
        }

        public async Task<Guid> AssignDataScopeAsync(AssignDataScopeCommand command)
        {
            try
            {
                _logger.LogInformation(
                    "Starting data scope assignment for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    command.AssigneeType, command.AssigneeId, command.ResourceId);

                // اعتبارسنجی وجود Resource
                var resource = await _resourceRepository.GetByIdAsync(command.ResourceId);
                if (resource == null)
                {
                    throw new ArgumentException($"Resource with ID {command.ResourceId} not found");
                }

                // اعتبارسنجی سلسله مراتب
                await ValidateDataScopeHierarchyAsync(command);

                // ایجاد DataScope جدید
                var dataScope = new DataScope(
                    command.ResourceId,
                    command.AssigneeType,
                    command.AssigneeId,
                    command.Scope,
                    command.SpecificUnitId,
                    command.CustomFilter,
                    command.Depth,
                    command.EffectiveFrom,
                    command.ExpiresAt,
                    command.Description,
                    createdBy: _currentUser.UserId?.ToString() ?? "system"
                );

                // ذخیره در Repository
                await _dataScopeRepository.AddAsync(dataScope);

                // انتشار ایونت
                dataScope.AddDomainEvent(new DataScopeChangedEvent(command.AssigneeId, command.ResourceId));

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش‌های مرتبط
                await InvalidateDataScopeCachesAsync(command.AssigneeId, command.ResourceId);

                _logger.LogInformation(
                    "Data scope assigned successfully: {DataScopeId} for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    dataScope.Id, command.AssigneeType, command.AssigneeId, command.ResourceId);

                return dataScope.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign data scope for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    command.AssigneeType, command.AssigneeId, command.ResourceId);
                throw;
            }
        }

        public async Task UpdateDataScopeAsync(UpdateDataScopeCommand command)
        {
            try
            {
                _logger.LogInformation("Starting data scope update: {DataScopeId}", command.DataScopeId);

                var dataScope = await _dataScopeRepository.GetByIdAsync(command.DataScopeId);
                if (dataScope == null)
                {
                    throw new ArgumentException($"DataScope with ID {command.DataScopeId} not found");
                }

                var assigneeId = dataScope.AssigneeId;
                var resourceId = dataScope.ResourceId;

                // اعتبارسنجی سلسله مراتب برای مقادیر جدید
                await ValidateDataScopeHierarchyAsync(new AssignDataScopeCommand(
                    resourceId,
                    dataScope.AssigneeType,
                    assigneeId,
                    command.Scope,
                    command.SpecificUnitId,
                    command.CustomFilter,
                    command.Depth
                ));

                // به‌روزرسانی DataScope
                dataScope.UpdateScope(
                    command.Scope,
                    command.SpecificUnitId,
                    command.CustomFilter,
                    command.Depth,
                    command.Description
                );

                // انتشار ایونت
                dataScope.AddDomainEvent(new DataScopeChangedEvent(assigneeId, resourceId));

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidateDataScopeCachesAsync(assigneeId, resourceId);

                _logger.LogInformation("Data scope updated successfully: {DataScopeId}", command.DataScopeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update data scope: {DataScopeId}", command.DataScopeId);
                throw;
            }
        }

        public async Task<DataScopeDto> GetDataScopeAsync(Guid dataScopeId)
        {
            try
            {
                var dataScope = await _dataScopeRepository.GetByIdAsync(dataScopeId);
                if (dataScope == null)
                {
                    _logger.LogWarning("Data scope not found: {DataScopeId}", dataScopeId);
                    return null;
                }

                var dto = MapToDto(dataScope);
                _logger.LogDebug("Retrieved data scope: {DataScopeId}", dataScopeId);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data scope: {DataScopeId}", dataScopeId);
                throw;
            }
        }

        public async Task<IReadOnlyList<DataScopeDto>> GetUserDataScopesAsync(Guid userId)
        {
            try
            {
                var spec = new DataScopesByUserSpec(userId);
                var dataScopes = await _dataScopeSpecRepository.ListBySpecAsync(spec);

                var dtos = dataScopes.Select(MapToDto).ToList();

                _logger.LogDebug(
                    "Retrieved {Count} data scopes for user {UserId}",
                    dtos.Count, userId);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving data scopes for user {UserId}", userId);
                throw;
            }
        }

        public async Task<string> BuildDataFilterAsync(Guid userId, string resourceKey)
        {
            try
            {
                // استفاده از DataScopeEvaluator تزریق شده
                var filter = await _dataScopeEvaluator.BuildDataFilterAsync(userId, resourceKey);

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

        public async Task ValidateDataScopeHierarchyAsync(AssignDataScopeCommand command)
        {
            try
            {
                // اعتبارسنجی SpecificUnitId برای ScopeType.SpecificUnit
                if (command.Scope == ScopeType.SpecificUnit && !command.SpecificUnitId.HasValue)
                {
                    throw new ArgumentException("SpecificUnitId is required for SpecificUnit scope");
                }

                // اعتبارسنجی SpecificUnitId برای سایر Scopeها
                if (command.Scope != ScopeType.SpecificUnit && command.SpecificUnitId.HasValue)
                {
                    throw new ArgumentException("SpecificUnitId should only be set for SpecificUnit scope");
                }

                // اعتبارسنجی عمق برای Subtree
                if (command.Scope == ScopeType.Subtree && (command.Depth < 1 || command.Depth > 10))
                {
                    throw new ArgumentException("Depth must be between 1 and 10 for Subtree scope");
                }

                _logger.LogDebug(
                    "Data scope hierarchy validation passed for resource {ResourceId} with scope {Scope}",
                    command.ResourceId, command.Scope);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Data scope hierarchy validation failed for resource {ResourceId} with scope {Scope}",
                    command.ResourceId, command.Scope);
                throw;
            }
        }

        private async Task InvalidateDataScopeCachesAsync(Guid assigneeId, Guid resourceId)
        {
            try
            {
                await _cache.RemoveByPatternAsync($"auth:datascope:{assigneeId}:*");
                await _cache.RemoveByPatternAsync($"auth:alldatascopes:{assigneeId}");
                await _cache.RemoveByPatternAsync($"auth:useraccess:{assigneeId}");

                _logger.LogDebug("Invalidated data scope caches for assignee {AssigneeId}", assigneeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating data scope caches for assignee {AssigneeId}", assigneeId);
            }
        }

        private DataScopeDto MapToDto(DataScope dataScope)
        {
            return new DataScopeDto
            {
                Id = dataScope.Id,
                ResourceId = dataScope.ResourceId,
                ResourceKey = dataScope.Resource?.Key ?? string.Empty,
                AssigneeType = dataScope.AssigneeType,
                AssigneeId = dataScope.AssigneeId,
                Scope = dataScope.Scope,
                SpecificUnitId = dataScope.SpecificUnitId,
                CustomFilter = dataScope.CustomFilter,
                Depth = dataScope.Depth,
                IsActive = dataScope.IsActive,
                EffectiveFrom = dataScope.EffectiveFrom,
                ExpiresAt = dataScope.ExpiresAt,
                Description = dataScope.Description,
                CreatedAt = dataScope.CreatedAt,
                CreatedBy = dataScope.CreatedBy
            };
        }
    }
*/
}