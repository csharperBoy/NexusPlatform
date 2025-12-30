using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Specifications;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Services
{
    public class ResourceTreeBuilder : IResourceTreeBuilder
    {
        private readonly ISpecificationRepository<Resource, Guid> _resourceSpecRepository;
        private readonly IPermissionEvaluator _permissionEvaluator;
        private readonly ILogger<ResourceTreeBuilder> _logger;
        private readonly ICacheService _cache;

        public ResourceTreeBuilder(
            ISpecificationRepository<Resource, Guid> resourceSpecRepository,
            IPermissionEvaluator permissionEvaluator,
            ILogger<ResourceTreeBuilder> logger,
            ICacheService cache)
        {
            _resourceSpecRepository = resourceSpecRepository;
            _permissionEvaluator = permissionEvaluator;
            _logger = logger;
            _cache = cache;
        }

        public async Task<IReadOnlyList<ResourceTreeDto>> BuildTreeAsync()
        {
            var cacheKey = "auth:resourcetree:full";

            try
            {
                var cached = await _cache.GetAsync<IReadOnlyList<ResourceTreeDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for full resource tree");
                    return cached;
                }

                var allResourcesSpec = new ResourceByCategorySpec();
                var allResources = await _resourceSpecRepository.ListBySpecAsync(allResourcesSpec);

                var tree = BuildTreeFromList(allResources, null);
                await _cache.SetAsync(cacheKey, tree, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Built full resource tree with {Count} root nodes", tree.Count);
                return tree;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building full resource tree");
                throw;
            }
        }

        public async Task<IReadOnlyList<ResourceTreeDto>> BuildTreeForUserAsync(Guid userId)
        {
            var cacheKey = $"auth:resourcetree:user:{userId}";

            try
            {
                var cached = await _cache.GetAsync<IReadOnlyList<ResourceTreeDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for user resource tree: {UserId}", userId);
                    return cached;
                }

                var allResourcesSpec = new ResourceByCategorySpec();
                var allResources = await _resourceSpecRepository.ListBySpecAsync(allResourcesSpec);

                var userPermissions = await _permissionEvaluator.EvaluateAllUserPermissionsAsync(userId);
                var accessibleResourceKeys = userPermissions
                    .Where(p => p.CanView)
                    .Select(p => p.ResourceKey)
                    .ToHashSet();

                var accessibleResources = allResources
                    .Where(r => accessibleResourceKeys.Contains(r.Key))
                    .ToList();

                var tree = BuildTreeFromList(accessibleResources, null);
                await _cache.SetAsync(cacheKey, tree, TimeSpan.FromMinutes(15));

                _logger.LogInformation(
                    "Built resource tree for user {UserId} with {Count} accessible root nodes",
                    userId, tree.Count);

                return tree;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building resource tree for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IReadOnlyList<ResourceDto>> GetResourcePathAsync(string resourceKey)
        {
            try
            {
                var resource = await GetResourceByKeyAsync(resourceKey);
                if (resource == null)
                {
                    _logger.LogWarning("Resource not found for path: {ResourceKey}", resourceKey);
                    return Array.Empty<ResourceDto>();
                }

                var path = await BuildResourcePathAsync(resource);

                _logger.LogDebug(
                    "Built resource path for {ResourceKey} with {Count} ancestors",
                    resourceKey, path.Count - 1);

                return path;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building resource path for {ResourceKey}", resourceKey);
                throw;
            }
        }

        private async Task<ResourceDto> GetResourceByKeyAsync(string key)
        {
            var spec = new ResourceByKeySpec(key);
            var resource = await _resourceSpecRepository.GetBySpecAsync(spec);
            return resource != null ? MapToDto(resource) : null;
        }

        private async Task<IReadOnlyList<ResourceDto>> BuildResourcePathAsync(ResourceDto resource)
        {
            var path = new List<ResourceDto> { resource };
            var current = resource;

            while (current.ParentId.HasValue)
            {
                var parentSpec = new ResourceByIdSpec(current.ParentId.Value);
                var parent = await _resourceSpecRepository.GetBySpecAsync(parentSpec);

                if (parent == null) break;

                var parentDto = MapToDto(parent);
                path.Insert(0, parentDto);
                current = parentDto;
            }

            return path.AsReadOnly();
        }

        private IReadOnlyList<ResourceTreeDto> BuildTreeFromList(IEnumerable<Resource> resources, Guid? parentId)
        {
            var nodes = resources
                .Where(r => r.ParentId == parentId)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.Name)
                .Select(resource => new ResourceTreeDto
                {
                    Id = resource.Id,
                    Key = resource.Key,
                    Name = resource.Name,
                    Description = resource.Description,
                    Type = resource.Type,
                    Category = resource.Category,
                    ParentId = resource.ParentId,
                    ParentKey = resource.Parent?.Key ?? string.Empty,
                    IsActive = resource.IsActive,
                    DisplayOrder = resource.DisplayOrder,
                    Icon = resource.Icon,
                    Route = resource.Route,
                    Path = resource.ResourcePath,
                    CreatedAt = resource.CreatedAt,
                    CreatedBy = resource.CreatedBy,
                    ModifiedAt = resource.ModifiedAt,
                    ModifiedBy = resource.ModifiedBy,
                    Children = BuildTreeFromList(resources, resource.Id)
                })
                .ToList();

            return nodes;
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
                ParentKey = resource.Parent?.Key ?? string.Empty,
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
    }
}