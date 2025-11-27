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
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

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

        public ResourceService(
            IRepository<AuthorizationDbContext, Resource, Guid> resourceRepository,
            ISpecificationRepository<Resource, Guid> resourceSpecRepository,
            IUnitOfWork<AuthorizationDbContext> unitOfWork,
            ILogger<ResourceService> logger,
            ICurrentUserService currentUser,
            ICacheService cache)
        {
            _resourceRepository = resourceRepository;
            _resourceSpecRepository = resourceSpecRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUser = currentUser;
            _cache = cache;
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

                var resource = await _resourceRepository.GetByIdAsync(command.Id);
                if (resource == null)
                {
                    throw new ArgumentException($"Resource with ID {command.Id} not found");
                }

                // به‌روزرسانی Resource
                resource.Update(
                    command.Name,
                    command.Description,
                    command.Type,
                    command.Category,
                    command.DisplayOrder,
                    command.Icon,
                    command.Route
                );

                // انتشار ایونت
                resource.AddDomainEvent(new ResourceHierarchyChangedEvent(resource.Id));

                // ذخیره تغییرات
                await _unitOfWork.SaveChangesAsync();

                // پاک کردن کش
                await InvalidateResourceCachesAsync();

                _logger.LogInformation("Resource updated successfully: {ResourceId}", command.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update resource: {ResourceId}", command.Id);
                throw;
            }
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
                Path = resource.Path,
                CreatedAt = resource.CreatedAt,
                CreatedBy = resource.CreatedBy,
                ModifiedAt = resource.ModifiedAt,
                ModifiedBy = resource.ModifiedBy
            };
        }
    }
}