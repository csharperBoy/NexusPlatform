using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Domain.Specifications;
using Authorization.Infrastructure.Data;
using Core.Application.Abstractions.Authorization;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Services
{
    public class SeedService : IAuthorizeSeedService
    {
        private AuthorizationDbContext _dbContext;
        private readonly ILogger<SeedService> _logger;

        public SeedService(AuthorizationDbContext dbContext, ILogger<SeedService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
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
        private async Task<Resource?> GetResourceEntityByKeyAsync(string key)
        {
            return await _dbContext.Resources.Where(r => r.Key == key).FirstOrDefaultAsync();

        }

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
                            def.Icon
                        );
                        newResource.GeneratePath();
                        // تنظیم Creator
                        // چون Seed است معمولاً System می‌زنیم، مگر اینکه در Context کاربر باشد
                        // newResource.SetCreatedBy("System"); 

                        await AddResourcesAsync(newResource);
                        keyToIdMap[def.Key] = newResource.Id; // نگهداری ID برای فرزندان احتمالی
                        _logger.LogDebug("➕ Added resource: {Key}", def.Key);
                    }
                    else
                    {
                        // --- UPDATE ---
                        // فقط در صورتی آپدیت می‌کنیم که تغییری کرده باشد
                        bool hasChanges = existingResource.Name != def.Name ||
                                          existingResource.ParentId != parentId ||
                                          existingResource.Description != def.Description ||
                                          existingResource.DisplayOrder != def.Order ||
                                          existingResource.Icon != def.Icon ||
                                          existingResource.Type != def.Type.ToEnumOrDefault(ResourceType.Ui) ||
                                          existingResource.Category != def.Category.ToEnumOrDefault(ResourceCategory.System)
                                          ;
                        // و سایر فیلدها...

                        if (hasChanges)
                        {
                            existingResource.Update(
                                def.Name,
                                def.Description,
                                def.Type.ToEnumOrDefault(ResourceType.Ui),
                                def.Category.ToEnumOrDefault(ResourceCategory.System),
                                def.Order,
                                def.Icon
                            );
                            // اگر والد تغییر کرده
                            if (existingResource.ParentId != parentId)
                            {
                                existingResource.ChangeParent(parentId);
                            }
                            existingResource.GeneratePath();

                            // اگر Path تغییر کرده (مهم برای Hierarchy)
                            //if (!string.IsNullOrEmpty(def.Path))
                            //{
                            //    existingResource.SetPath(def.Path);
                            //}

                            UpdateResourcesAsync(existingResource);
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
            await SaveChangesAsync(cancellationToken);

            // 5. پاکسازی کش
            //await InvalidateResourceCachesAsync();

            _logger.LogInformation("✅ Resource sync completed successfully.");
        }

        private async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync();
        }

        private void UpdateResourcesAsync(Resource existingResource)
        {
            _dbContext.Resources.Update(existingResource);
        }

        private async Task AddResourcesAsync(Resource newResource)
        {
            await _dbContext.Resources.AddAsync(newResource);
        }


        public async Task SeedRolePermissionsAsync(List<PermissionDefinition> permissions, CancellationToken cancellationToken = default)
        {
            foreach (var permissionDefinition in permissions)
            {
                var specRet = await GetResourceEntityByKeyAsync(permissionDefinition.ResourceKey);
                Permission permission = new Permission(
                    specRet.Id, permissionDefinition.AssignType.ToEnumOrDefault(AssigneeType.Role),
                    permissionDefinition.AssignId,
                    permissionDefinition.Action.ToEnumOrDefault(Core.Shared.Enums.Authorization.PermissionAction.View),
                    //permissionDefinition.Scope.ToEnumOrDefault(ScopeType.Self),
                    //null,
                    permissionDefinition.Effect .ToEnumOrDefault(PermissionEffect.allow)
                    );

                if (!IsPermissionExist(permission))
                    await AddPermissionAsync(permission);

            }
            await SaveChangesAsync(cancellationToken);
        }

        private async Task AddPermissionAsync(Permission permission)
        {
            await _dbContext.Permissions.AddAsync(permission);
        }

        private bool IsPermissionExist(Permission permission)
        {
            try
            {
                return _dbContext.Permissions.Any(
                    p => p.ResourceId == permission.ResourceId &&
                    //p.Scope == permission.Scope &&
                    //p.SpecificScopeId == permission.SpecificScopeId && // همیشه مقدار دارد
                    p.Action == permission.Action &&
                    p.AssigneeType == permission.AssigneeType &&
                    p.AssigneeId == permission.AssigneeId &&
                    p.Effect == permission.Effect
                    );
            }
            catch (Exception ex)
            {

                throw;
            }


        }
    }
}
