using Authorization.Application.Commands;
using Authorization.Application.Commands.Resource;
using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Authorization.Domain.Specifications;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Security;
using Core.Domain.Interfaces;
using Identity.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Authorization.Application.DTOs.Resource.Extensions;

namespace Authorization.Infrastructure.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IRepository<AuthorizationDbContext, Resource, Guid> _resourceRepository;
        private readonly ISpecificationRepository<Resource, Guid> _resourceSpecRepository;
        private readonly IUnitOfWork<AuthorizationDbContext> _unitOfWork;
        private readonly ILogger<ResourceService> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly ICacheService _cache;
        private readonly IServiceScopeFactory _scopeFactory;

        public ResourceService(
            IRepository<AuthorizationDbContext, Resource, Guid> resourceRepository,
            ISpecificationRepository<Resource, Guid> resourceSpecRepository,
            IUnitOfWork<AuthorizationDbContext> unitOfWork,
            ILogger<ResourceService> logger,
            ICurrentUserService currentUser,
            ICacheService cache,
            IServiceScopeFactory scopeFactory)
        {
            _resourceRepository = resourceRepository;
            _resourceSpecRepository = resourceSpecRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUser = currentUser;
            _cache = cache;
            _scopeFactory = scopeFactory;
        }

        public async Task<Guid> CreateResourceAsync(CreateResourceCommand command)
        {
            try
            {
                _logger.LogInformation(
                    "Starting resource creation: {ResourceKey} ({ResourceName})",
                    command.Key, command.Name);

                // بررسی تکراری نبودن کلید
                var existingResource = await GetResourceByKeyAsync(command.Key);
                if (existingResource != null)
                {
                    throw new ArgumentException($"Resource with key '{command.Key}' already exists");
                }

                // اعتبارسنجی سلسله مراتب والد
                if (command.ParentId.HasValue)
                {
                    var parentExists = await _resourceRepository.ExistsAsync(r => r.Id == command.ParentId.Value);
                    if (!parentExists)
                    {
                        throw new ArgumentException($"Parent resource with ID {command.ParentId} not found");
                    }
                }

                // ایجاد Resource جدید
                var resource = new Resource(
                    command.Key,
                    command.Name,
                    command.Type,
                    command.Category,
                    command.ParentId,
                    command.Description,
                    command.DisplayOrder,
                    command.Icon,
                    command.Route,
                    createdBy: _currentUser.UserId?.ToString() ?? "system"
                );

                // ذخیره در Repository
                await _resourceRepository.AddAsync(resource);

                // انتشار ایونت
                resource.AddDomainEvent(new ResourceHierarchyChangedEvent(resource.Id));

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش‌های مرتبط
                await InvalidateResourceCachesAsync();

                _logger.LogInformation(
                    "Resource created successfully: {ResourceId} ({ResourceKey})",
                    resource.Id, resource.Key);

                return resource.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create resource: {ResourceKey} ({ResourceName})",
                    command.Key, command.Name);
                throw;
            }
        }

        public async Task UpdateResourceAsync(UpdateResourceCommand command)
        {
            try
            {
                _logger.LogInformation("Starting resource update: {ResourceId}", command.Id);

                // 🔵 دریافت resource
                var resource = await _resourceRepository.GetByIdAsync(command.Id);
                if (resource == null)
                {
                    throw new ArgumentException($"Resource with ID {command.Id} not found");
                }

                // 🔴 اعتبارسنجی تغییر والد (اگر تغییر کرده)
                if (command.ParentId != resource.ParentId)
                {
                    await ValidateAndApplyParentChangeAsync(resource, command.ParentId, command.Id);
                }

                // 🔵 به‌روزرسانی مشخصات اصلی
                resource.Update(
                    command.Name,
                    command.Description,
                    command.Type,
                    command.Category,
                    command.DisplayOrder,
                    command.Icon,
                    command.Route
                );

                // 🔵 انتشار ایونت
                resource.AddDomainEvent(new ResourceHierarchyChangedEvent(resource.Id));

                // 🔵 ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // 🔵 پاک کردن کش
                await InvalidateResourceCachesAsync();

                _logger.LogInformation("Resource updated successfully: {ResourceId}", command.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update resource: {ResourceId}", command.Id);
                throw;
            }
        }

        // 🔵 متد کمکی: اعتبارسنجی و اعمال تغییر والد
        private async Task ValidateAndApplyParentChangeAsync(
            Resource resource,
            Guid? newParentId,
            Guid resourceId)
        {
            _logger.LogInformation(
                "Validating parent change for resource {ResourceId}: {OldParentId} -> {NewParentId}",
                resourceId, resource.ParentId, newParentId);

            // 1. اعتبارسنجی سلسله مراتب
            var isValid = await ValidateResourceHierarchyAsync(resourceId, newParentId);
            if (!isValid)
            {
                throw new InvalidOperationException(
                    $"Cannot change parent for resource '{resource.Key}'. " +
                    "Hierarchy validation failed. Possible circular reference or invalid parent.");
            }

            // 2. بررسی وجود والد جدید (اگر null نیست)
            if (newParentId.HasValue)
            {
                var parentExists = await _resourceRepository.ExistsAsync(r => r.Id == newParentId.Value);
                if (!parentExists)
                {
                    throw new ArgumentException($"Parent resource with ID {newParentId.Value} not found");
                }
            }

            // 3. اعمال تغییر والد
            resource.ChangeParent(newParentId);

            _logger.LogInformation(
                "Parent change validated and applied for resource {ResourceId}",
                resourceId);
        }
        public async Task DeleteResourceAsync(Guid resourceId)
        {
            try
            {
                _logger.LogInformation("Starting resource deletion: {ResourceId}", resourceId);

                var resource = await _resourceRepository.GetByIdAsync(resourceId);
                if (resource == null)
                {
                    throw new ArgumentException($"Resource with ID {resourceId} not found");
                }

                // بررسی وجود زیرمجموعه
                var hasChildren = await _resourceRepository.ExistsAsync(r => r.ParentId == resourceId);
                if (hasChildren)
                {
                    throw new InvalidOperationException(
                        $"Cannot delete resource {resourceId} because it has child resources");
                }

                // حذف Resource
                await _resourceRepository.DeleteAsync(resourceId);

                // انتشار ایونت
                resource.AddDomainEvent(new ResourceHierarchyChangedEvent(resource.Id));

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidateResourceCachesAsync();

                _logger.LogInformation("Resource deleted successfully: {ResourceId}", resourceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete resource: {ResourceId}", resourceId);
                throw;
            }
        }

        public async Task<ResourceDto> GetResourceAsync(Guid resourceId)
        {
            try
            {
                var resource = await _resourceRepository.GetByIdAsync(resourceId);
                if (resource == null)
                {
                    _logger.LogWarning("Resource not found: {ResourceId}", resourceId);
                    return null;
                }

                var dto = MapToDto(resource);
                _logger.LogDebug("Retrieved resource: {ResourceId}", resourceId);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource: {ResourceId}", resourceId);
                throw;
            }
        }

        public async Task<ResourceDto> GetResourceByKeyAsync(string key)
        {
            try
            {
                var spec = new ResourceByKeySpec(key);
                var resource = await _resourceSpecRepository.GetBySpecAsync(spec);

                if (resource == null)
                {
                    _logger.LogDebug("Resource not found by key: {ResourceKey}", key);
                    return null;
                }

                var dto = MapToDto(resource);
                _logger.LogDebug("Retrieved resource by key: {ResourceKey}", key);

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving resource by key: {ResourceKey}", key);
                throw;
            }
        }

        public async Task<bool> ValidateResourceHierarchyAsync(Guid resourceId, Guid? newParentId)
        {
            try
            {
                if (!newParentId.HasValue)
                {
                    _logger.LogDebug("Resource hierarchy validation passed: no parent specified");
                    return true; // ریشه درخت
                }

                // بررسی وجود والد
                var parentExists = await _resourceRepository.ExistsAsync(r => r.Id == newParentId.Value);
                if (!parentExists)
                {
                    _logger.LogWarning("Parent resource not found: {ParentId}", newParentId.Value);
                    return false;
                }

                // بررسی circular reference
                if (resourceId == newParentId.Value)
                {
                    _logger.LogWarning("Circular reference detected: resource cannot be its own parent");
                    return false;
                }

                // بررسی اینکه والد جدید از نوادگان نباشد
                var isDescendant = await IsDescendantAsync(newParentId.Value, resourceId);
                if (isDescendant)
                {
                    _logger.LogWarning(
                        "Hierarchy violation: new parent {ParentId} is a descendant of resource {ResourceId}",
                        newParentId.Value, resourceId);
                    return false;
                }

                _logger.LogDebug(
                    "Resource hierarchy validation passed for resource {ResourceId} with parent {ParentId}",
                    resourceId, newParentId.Value);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error validating resource hierarchy for resource {ResourceId} with parent {ParentId}",
                    resourceId, newParentId.Value);
                throw;
            }
        }

        public async Task RebuildResourceTreeAsync()
        {
            try
            {
                _logger.LogInformation("Starting resource tree rebuild");

                // دریافت تمام منابع
                var allResources = await _resourceRepository.GetAllAsync();

                // بازسازی مسیرها برای تمام منابع
                foreach (var resource in allResources)
                {
                    // فراخوانی GeneratePath برای هر Resource
                    resource.GeneratePath();
                }

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidateResourceCachesAsync();

                _logger.LogInformation("Resource tree rebuilt successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rebuild resource tree");
                throw;
            }
        }

        private async Task<bool> IsDescendantAsync(Guid potentialAncestorId, Guid resourceId)
        {
            // پیاده‌سازی بازگشتی برای بررسی سلسله مراتب
            var current = await _resourceRepository.GetByIdAsync(resourceId);
            while (current?.ParentId != null)
            {
                if (current.ParentId == potentialAncestorId)
                {
                    return true;
                }
                current = await _resourceRepository.GetByIdAsync(current.ParentId.Value);
            }
            return false;
        }

        private async Task InvalidateResourceCachesAsync()
        {
            try
            {
                await _cache.RemoveByPatternAsync("auth:resource:*");
                await _cache.RemoveByPatternAsync("auth:resourcetree:*");
                await _cache.RemoveByPatternAsync("auth:useraccess:*");

                _logger.LogDebug("Invalidated resource caches");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating resource caches");
            }
        }

        private ResourceDto MapToDto(Resource resource)
        {
            return new ResourceDto
            {
                Id = resource.Id,
                Key = resource.Key,
                Name = resource.Name,
                Description = resource.Description,
                Type = resource.Type,
                Category = resource.Category,
                ParentId = resource.ParentId,
                IsActive = resource.IsActive,
                DisplayOrder = resource.DisplayOrder,
                Icon = resource.Icon,
                Route = resource.Route,
                Path = resource.ResourcePath,
                CreatedAt = resource.CreatedAt,
                CreatedBy = resource.CreatedBy,
                ModifiedAt = resource.ModifiedAt,
                ModifiedBy = resource.ModifiedBy
            };
        }

        public async Task RegisterModuleResourcesAsync(string moduleKey)
        {
            using var scope = _scopeFactory.CreateScope();
            var providers = scope.ServiceProvider.GetServices<IResourceDefinitionProvider>();
            var provider = providers.FirstOrDefault(p => p.ModuleKey == moduleKey);

            if (provider == null)
            {
                _logger.LogWarning("No resource definition provider found for module: {ModuleKey}", moduleKey);
                return;
            }

            await RegisterResourcesAsync(provider);
        }

        public async Task RegisterAllModulesResourcesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var providers = scope.ServiceProvider.GetServices<IResourceDefinitionProvider>();

            _logger.LogInformation("Found {Count} resource definition providers", providers.Count());

            foreach (var provider in providers)
            {
                await RegisterResourcesAsync(provider);
            }
        }

        public async Task SyncResourcesWithDefinitionsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var providers = scope.ServiceProvider.GetServices<IResourceDefinitionProvider>();
            var allDefinedResources = new List<ResourceDefinition>();

            foreach (var provider in providers)
            {
                var resources = provider.GetResourceDefinitions();
                allDefinedResources.AddRange(resources);
            }

            await SyncResourcesAsync(allDefinedResources);
        }
        public async Task<Guid> CreateOrUpdateResourceFromDefinitionAsync(ResourceDefinition definition)
        {
            _logger.LogInformation(
                "Processing resource definition: {ResourceKey} ({ResourceName})",
                definition.Key, definition.Name);

            // 🔵 مرحله 1: تبدیل ResourceDefinition
            var resourceDto = definition.ToResourceDto();

            // 🔵 مرحله 2: پیدا کردن والد با ParentKey
            Guid? parentId = await ResolveParentIdFromParentKeyAsync(definition.ParentKey, definition.Key);

            // 🔵 مرحله 3: بررسی وجود resource
            var existingResource = await GetResourceByKeyAsync(definition.Key);

            if (existingResource == null)
            {
                // 🔴 حالت 1: ایجاد resource جدید
                return await CreateResourceFromDefinitionAsync(definition, resourceDto, parentId);
            }
            else
            {
                // 🔴 حالت 2: به‌روزرسانی resource موجود
                return await UpdateResourceFromDefinitionAsync(
                    definition, resourceDto, parentId, existingResource);
            }
        }

        // 🔵 متد کمکی: پیدا کردن ParentId از ParentKey
        private async Task<Guid?> ResolveParentIdFromParentKeyAsync(string parentKey, string resourceKey)
        {
            if (string.IsNullOrEmpty(parentKey))
            {
                _logger.LogDebug("No parent specified for resource: {ResourceKey}", resourceKey);
                return null;
            }

            var parentResource = await GetResourceByKeyAsync(parentKey);
            if (parentResource == null)
            {
                _logger.LogWarning(
                    "Parent resource with key '{ParentKey}' not found for resource '{ResourceKey}'. " +
                    "Resource will be created without parent or with invalid parent reference.",
                    parentKey, resourceKey);
                return null;
            }

            _logger.LogDebug(
                "Resolved parent: {ParentKey} -> {ParentId} for resource: {ResourceKey}",
                parentKey, parentResource.Id, resourceKey);

            return parentResource.Id;
        }

        // 🔵 متد کمکی: ایجاد resource جدید
        private async Task<Guid> CreateResourceFromDefinitionAsync(
            ResourceDefinition definition,
            ResourceDto resourceDto,
            Guid? parentId)
        {
            _logger.LogInformation(
                "Creating new resource from definition: {ResourceKey}",
                definition.Key);

            var createCommand = new CreateResourceCommand(
                Key: definition.Key,
                Name: definition.Name,
                Type: resourceDto.Type,
                Category: resourceDto.Category,
                ParentId: parentId,
                Description: definition.Description,
                DisplayOrder: definition.DisplayOrder,
                Icon: definition.Icon,
                Route: definition.Route
            );

            var resourceId = await CreateResourceAsync(createCommand);

            _logger.LogInformation(
                "Created new resource: {ResourceKey} -> {ResourceId}",
                definition.Key, resourceId);

            return resourceId;
        }

        // 🔵 متد کمکی: به‌روزرسانی resource موجود
        private async Task<Guid> UpdateResourceFromDefinitionAsync(
            ResourceDefinition definition,
            ResourceDto resourceDto,
            Guid? parentId,
            ResourceDto existingResource)
        {
            _logger.LogInformation(
                "Updating existing resource from definition: {ResourceKey} ({ResourceId})",
                definition.Key, existingResource.Id);

            // 🔴 بررسی آیا والد تغییر کرده؟
            bool parentChanged = existingResource.ParentId != parentId;

            if (parentChanged)
            {
                _logger.LogInformation(
                    "Parent changed for resource {ResourceKey}: {OldParentId} -> {NewParentId}",
                    definition.Key, existingResource.ParentId, parentId);
            }

            var updateCommand = new UpdateResourceCommand(
                Id: existingResource.Id,
                Name: definition.Name,
                Description: definition.Description,
                Type: resourceDto.Type,
                Category: resourceDto.Category,
                DisplayOrder: definition.DisplayOrder,
                Icon: definition.Icon,
                Route: definition.Route,
                ParentId: parentId // ✅ ارسال ParentId
            );

            await UpdateResourceAsync(updateCommand);

            _logger.LogInformation(
                "Updated resource: {ResourceKey} ({ResourceId})",
                definition.Key, existingResource.Id);

            return existingResource.Id;
        }
        // متدهای کمکی private

        private async Task RegisterResourcesAsync(IResourceDefinitionProvider provider)
        {
            _logger.LogInformation(
                "Registering resources for module: {ModuleName} ({ModuleKey})",
                provider.ModuleName, provider.ModuleKey);

            var resources = provider.GetResourceDefinitions().ToList();
            var successfulRegistrations = 0;
            var failedRegistrations = 0;

            foreach (var resourceDefinition in resources)
            {
                try
                {
                    await CreateOrUpdateResourceFromDefinitionAsync(resourceDefinition);
                    successfulRegistrations++;

                    _logger.LogDebug(
                        "Successfully registered resource: {ResourceKey} for module {ModuleKey}",
                        resourceDefinition.Key, provider.ModuleKey);
                }
                catch (Exception ex)
                {
                    failedRegistrations++;

                    _logger.LogError(
                        ex,
                        "Failed to register resource: {ResourceKey} for module {ModuleKey}",
                        resourceDefinition.Key, provider.ModuleKey);
                }
            }

            // 🔵 ذخیره همه تغییرات به صورت batch
            try
            {
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Resource registration completed for module {ModuleName}: " +
                    "{SuccessCount} successful, {FailedCount} failed",
                    provider.ModuleName, successfulRegistrations, failedRegistrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to save changes for module {ModuleName} resource registration",
                    provider.ModuleName);
                throw;
            }
        }

        private async Task SyncResourcesAsync(List<ResourceDefinition> definedResources)
        {
            var existingResources = await _resourceRepository.GetAllAsync();

            // اضافه کردن یا به‌روزرسانی منابع تعریف شده
            foreach (var resourceDefinition in definedResources)
            {
                await CreateOrUpdateResourceFromDefinitionAsync(resourceDefinition);
            }

            // غیرفعال کردن منابعی که دیگر تعریف نشده‌اند (به جز منابع سیستمی)
            var definedKeys = definedResources.Select(r => r.Key).ToList();
            var orphanedResources = existingResources
                .Where(r => !definedKeys.Contains(r.Key) && r.Category != ResourceCategory.System)
                .ToList();

            foreach (var orphanedResource in orphanedResources)
            {
                orphanedResource.Deactivate();
                _logger.LogWarning("Deactivated orphaned resource: {ResourceKey} - {ResourceName}",
                    orphanedResource.Key, orphanedResource.Name);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Resource synchronization completed. {ActiveCount} active, {InactiveCount} inactive",
                existingResources.Count(r => r.IsActive), orphanedResources.Count);
        }
    }
}