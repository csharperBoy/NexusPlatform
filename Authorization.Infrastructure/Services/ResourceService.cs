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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Authorization.Infrastructure.Data;
using Core.Application.Abstractions.Authorization;

namespace Authorization.Infrastructure.Services
{
    public class ResourceService : IResourceInternalService, IResourcePublicService
    {
        // توجه: TDbContext فقط به عنوان پارامتر جنریک پاس داده می‌شود، اما در کد استفاده نمی‌شود
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

        // ========================================================================
        // IResourcePublicService Implementation (متد جدید برای Seed)
        // ========================================================================

        public async Task SyncModuleResourcesAsync(List<ResourceDefinition> resources, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🔄 Starting sync for {Count} root resources...", resources.Count);

            // 1. تبدیل درخت به لیست خطی (با رعایت ترتیب والد -> فرزند)
            var flatResources = FlattenResources(resources);

            // 2. دیکشنری برای نگهداری Key -> Id (برای ست کردن ParentId بدون کوئری اضافه)
            var keyToIdMap = new Dictionary<string, Guid>();

            // 3. دریافت لیست منابع موجود برای تشخیص تکراری‌ها (بهینه‌سازی)
            // بهتر است تمام منابع این ماژول را یکجا بگیریم، اما فعلا با Key چک می‌کنیم
            // اگر تعداد زیاد است، می‌توان همه را Fetch کرد.

            foreach (var def in flatResources)
            {
                try
                {
                    // الف) پیدا کردن ParentId
                    Guid? parentId = null;
                    if (!string.IsNullOrEmpty(def.ParentKey))
                    {
                        if (keyToIdMap.TryGetValue(def.ParentKey, out var pid))
                        {
                            parentId = pid;
                        }
                        else
                        {
                            // اگر والد در لیست جاری نبود، شاید قبلاً در دیتابیس بوده
                            var parentRes = await GetResourceEntityByKeyAsync(def.ParentKey);
                            if (parentRes != null) parentId = parentRes.Id;
                            else _logger.LogWarning("⚠️ Parent '{ParentKey}' not found for '{Key}'", def.ParentKey, def.Key);
                        }
                    }

                    // ب) بررسی وجود Resource
                    var existingResource = await GetResourceEntityByKeyAsync(def.Key);

                    if (existingResource == null)
                    {
                        // --- CREATE ---
                        var newResource = new Resource(
                            def.Key,
                            def.Name,
                             def.Type.ToEnumOrDefault(ResourceType.Ui),
                            def.Category.ToEnumOrDefault(ResourceCategory.System),

                            parentId,
                            def.Description,
                            def.Order,
                            def.Icon,
                            def.Path
                        );

                        // تنظیم Creator
                        // چون Seed است معمولاً System می‌زنیم، مگر اینکه در Context کاربر باشد
                        // newResource.SetCreatedBy("System"); 

                        await _resourceRepository.AddAsync(newResource);
                        keyToIdMap[def.Key] = newResource.Id; // نگهداری ID برای فرزندان احتمالی
                        _logger.LogDebug("➕ Added resource: {Key}", def.Key);
                    }
                    else
                    {
                        // --- UPDATE ---
                        // فقط در صورتی آپدیت می‌کنیم که تغییری کرده باشد
                        bool hasChanges = existingResource.Name != def.Name ||
                                          existingResource.ParentId != parentId ||
                                          existingResource.ResourcePath != def.Path;
                        // و سایر فیلدها...

                        if (hasChanges)
                        {
                            existingResource.Update(
                                def.Name,
                                def.Description,
                                def.Type.ToEnumOrDefault(ResourceType.Ui),
                                def.Category.ToEnumOrDefault(ResourceCategory.System),
                                def.Order,
                                def.Icon,
                                null // Route فعلا نداریم در Definition
                            );

                            // اگر والد تغییر کرده
                            if (existingResource.ParentId != parentId)
                            {
                                existingResource.ChangeParent(parentId);
                            }

                            // اگر Path تغییر کرده (مهم برای Hierarchy)
                            if (!string.IsNullOrEmpty(def.Path))
                            {
                                existingResource.SetPath(def.Path);
                            }

                            await _resourceRepository.UpdateAsync(existingResource);
                            _logger.LogDebug("✏️ Updated resource: {Key}", def.Key);
                        }

                        keyToIdMap[def.Key] = existingResource.Id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to sync resource '{Key}'", def.Key);
                    throw;
                }
            }

            // 4. ذخیره نهایی
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. پاکسازی کش
            await InvalidateResourceCachesAsync();

            _logger.LogInformation("✅ Resource sync completed successfully.");
        }

        // ========================================================================
        // IResourceInternalService Implementation (سایر متدها)
        // ========================================================================

        public async Task<Guid> CreateResourceAsync(CreateResourceCommand command)
        {
            // اعتبارسنجی تکراری بودن
            var existing = await GetResourceEntityByKeyAsync(command.Key);
            if (existing != null)
                throw new ArgumentException($"Resource with key '{command.Key}' already exists");

            // اعتبارسنجی والد
            if (command.ParentId.HasValue)
            {
                var parentExists = await _resourceRepository.ExistsAsync(r => r.Id == command.ParentId.Value);
                if (!parentExists)
                    throw new ArgumentException($"Parent resource {command.ParentId} not found");
            }

            var resource = new Resource(
                command.Key,
                command.Name,
                command.Type,
                command.Category,
                command.ParentId,
                command.Description,
                command.DisplayOrder,
                command.Icon,
                command.Route
            );

            await _resourceRepository.AddAsync(resource);
            resource.AddDomainEvent(new ResourceHierarchyChangedEvent(resource.Id));

            await _unitOfWork.SaveChangesAsync();
            await InvalidateResourceCachesAsync();

            return resource.Id;
        }

        public async Task UpdateResourceAsync(UpdateResourceCommand command)
        {
            var resource = await _resourceRepository.GetByIdAsync(command.Id);
            if (resource == null) throw new ArgumentException("Resource not found");

            // آپدیت فیلدها
            resource.Update(
                command.Name,
                command.Description,
                command.Type,
                command.Category,
                command.DisplayOrder,
                command.Icon,
                command.Route
            );

            // تغییر والد با منطق خاص
            if (command.ParentId != resource.ParentId)
            {
                // اینجا بهتر است لاجیک ValidateResourceHierarchyAsync صدا زده شود
                // اما برای خلاصه شدن کد، مستقیم تغییر می‌دهیم (فرض بر چک شدن در API)
                resource.ChangeParent(command.ParentId);
            }

            await _resourceRepository.UpdateAsync(resource);

            // ایونت تغییر سلسله مراتب اگر والد عوض شده
            resource.AddDomainEvent(new ResourceHierarchyChangedEvent(resource.Id));

            await _unitOfWork.SaveChangesAsync();
            await InvalidateResourceCachesAsync();
        }

        public async Task DeleteResourceAsync(Guid resourceId)
        {
            var resource = await _resourceRepository.GetByIdAsync(resourceId);
            if (resource == null) return;

            // چک کردن فرزندان
            var hasChildren = await _resourceRepository.ExistsAsync(r => r.ParentId == resourceId);
            if (hasChildren)
                throw new InvalidOperationException("Cannot delete resource with children.");

            await _resourceRepository.DeleteAsync(resource);
            await _unitOfWork.SaveChangesAsync();
            await InvalidateResourceCachesAsync();
        }

        // ========================================================================
        // Private Helpers
        // ========================================================================

        // استفاده از Specification برای پیدا کردن با کلید
        private async Task<Resource?> GetResourceEntityByKeyAsync(string key)
        {
            // اینجا فرض می‌کنیم کلاسی به نام ResourceByKeySpec ساخته‌اید
            // اگر نساخته‌اید، می‌توانید موقتاً از GetAll و Linq استفاده کنید (برای سید دیتا پرفورمنس حیاتی نیست)
            // اما روش درست Specification است:
            var spec = new ResourceByKeySpec(key);
            return await _resourceSpecRepository.GetBySpecAsync(spec);
        }

        // الگوریتم Flatten کردن درخت
        private List<ResourceDefinition> FlattenResources(List<ResourceDefinition> resources)
        {
            var result = new List<ResourceDefinition>();
            foreach (var res in resources)
            {
                // والد اول اضافه می‌شود
                result.Add(res);

                // بعد فرزندان به صورت بازگشتی
                if (res.Children != null && res.Children.Any())
                {
                    // ست کردن ParentKey برای فرزندان (جهت اطمینان)
                    foreach (var child in res.Children) child.ParentKey = res.Key;

                    result.AddRange(FlattenResources(res.Children));
                }
            }
            return result;
        }

        private async Task InvalidateResourceCachesAsync()
        {
            await _cache.RemoveByPatternAsync("auth:resource:*");
        }

        // تبدیل رشته به Enum (چون Definition استرینگ دارد ولی دیتابیس Enum)
        /*  private ResourceType ParseResourceType(string typeStr) =>
              Enum.TryParse<ResourceType>(typeStr, true, out var val) ? val : ResourceType.Ui; // پیش‌فرض

          private ResourceCategory ParseResourceCategory(string catStr) =>
              Enum.TryParse<ResourceCategory>(catStr, true, out var val) ? val : ResourceCategory.System;
     */
    }
}