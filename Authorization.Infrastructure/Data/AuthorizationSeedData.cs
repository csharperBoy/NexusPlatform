using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Authorization.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Core.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization.Infrastructure.Data
{

    public static class AuthorizationSeedData
    {
       // تعریف منابع به صورت درختی (Hierarchical)
        private static List<ResourceDefinition> GetResourceDefinitions()
        {
            // ساختار درختی منابع
            return new List<ResourceDefinition>
        {
            
            new()
            {
                Key = "authorization",
                Name = "Authorization",
                Type = "Module",
                Category = "System",
                Description = "Authorization System administration",
                Order = 1000,
                Icon = "settings",
                Path = "/authorization",
                Children = new List<ResourceDefinition>
                {
                    new()
                    {
                        Key = "authorization.resource",
                        Name = "resource Management",
                        Type = "Data",
                        Category = "System",
                        Description = "Manage resource",
                        Order = 1001,
                        Icon = "users",
                        Path = "/authorization/resource"
                        
                    }
                }
            }
        };
        }

        // تبدیل ساختار درختی به لیست مسطح با حفظ سلسله مراتب
        private static List<(ResourceDefinition Definition, int Level)> FlattenResourceTree(
            List<ResourceDefinition> definitions, int level = 0)
        {
            var result = new List<(ResourceDefinition, int)>();

            foreach (var def in definitions)
            {
                result.Add((def, level));
                if (def.Children.Any())
                {
                    result.AddRange(FlattenResourceTree(def.Children, level + 1));
                }
            }

            return result;
        }

        // ایجاد نقشه کلید به Resource (برای بازیابی سریع)
        private static async Task<Dictionary<string, Resource>> GetExistingResourcesMap(
            AuthorizationDbContext dbContext)
        {
            return await dbContext.Set<Resource>()
                .ToDictionaryAsync(r => r.Key);
        }

        // متد اصلی برای Seed کردن منابع
        public static async Task SeedResourcesAsync(
            AuthorizationDbContext dbContext,
            IConfiguration config,
            ILogger logger)
        {
            logger.LogInformation("🚀 Starting optimized resource seeding...");

            try
            {
                var flatDefinitions = FlattenResourceTree(GetResourceDefinitions());

                // 1. دریافت تمام کلیدهای موجود
                var existingKeys = await dbContext.Set<Resource>()
                    .Select(r => r.Key)
                    .ToHashSetAsync();

                // 2. فیلتر کردن منابع جدید
                var newDefinitions = flatDefinitions
                    .Where(x => !existingKeys.Contains(x.Definition.Key))
                    .ToList();

                if (!newDefinitions.Any())
                {
                    logger.LogInformation("ℹ️ All resources already exist");
                    return;
                }

                // 3. ایجاد dictionary برای نگهداری کلید به Id
                var allResources = await dbContext.Set<Resource>()
                    .ToDictionaryAsync(r => r.Key, r => r.Id);

                // 4. ایجاد منابع جدید
                var newResources = new List<Resource>();

                foreach (var (definition, level) in newDefinitions)
                {
                    Guid? parentId = null;
                    if (!string.IsNullOrEmpty(definition.ParentKey) &&
                        allResources.TryGetValue(definition.ParentKey, out var pid))
                    {
                        parentId = pid;
                    }

                    var resource = new Resource(
                        definition.Key,
                        definition.Name,
                        definition.Type.ToEnumOrDefault(ResourceType.Ui),
                            definition.Category.ToEnumOrDefault(ResourceCategory.System),
                            parentId,
                        definition.Description,
                        definition.Order,
                        definition.Icon,
                        definition.Path
                    );
                    newResources.Add(resource);
                }

                // 5. استفاده از Bulk Insert (اگر از کتابخانه‌ای مثل EFCore.BulkExtensions استفاده می‌کنید)
                // یا AddRange معمولی
                await dbContext.Set<Resource>().AddRangeAsync(newResources);
                await dbContext.SaveChangesAsync();

                logger.LogInformation($"✅ Created {newResources.Count} new resources");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during resource seeding");
                throw;
            }
        }

        // پردازش یک تعریف Resource
        private static async Task ProcessResourceDefinition(
            ResourceDefinition definition,
            int level,
            Dictionary<string, Resource> existingResources,
            Dictionary<string, Guid?> parentKeyToIdMap,
            AuthorizationDbContext dbContext,
            ILogger logger)
        {
            try
            {
                // بررسی وجود Resource
                if (existingResources.ContainsKey(definition.Key))
                {
                    logger.LogDebug($"ℹ️ Resource '{definition.Key}' already exists, skipping...");
                    return;
                }

                // پیدا کردن ParentId
                Guid? parentId = null;
                if (!string.IsNullOrEmpty(definition.ParentKey))
                {
                    if (parentKeyToIdMap.TryGetValue(definition.ParentKey, out var pid))
                    {
                        parentId = pid;
                    }
                    else
                    {
                        // اگر ParentKey مشخص شده اما در مپ نیست، سعی می‌کنیم از منابع موجود پیدا کنیم
                        if (existingResources.TryGetValue(definition.ParentKey, out var parentResource))
                        {
                            parentId = parentResource.Id;
                            parentKeyToIdMap[definition.ParentKey] = parentId;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Parent resource with key '{definition.ParentKey}' not found for '{definition.Key}'");
                        }
                    }
                }

                // ایجاد Resource جدید
                var resource = new Resource(
                    definition.Key,
                    definition.Name,
                    definition.Type.ToEnumOrDefault(ResourceType.Ui),
                            definition.Category.ToEnumOrDefault(ResourceCategory.System),
                            parentId,
                    definition.Description,
                    definition.Order,
                    definition.Icon,
                    definition.Path
                );

                // اضافه کردن به DbContext
                await dbContext.Set<Resource>().AddAsync(resource);

                // به‌روزرسانی مپ‌ها
                existingResources[definition.Key] = resource;
                parentKeyToIdMap[definition.Key] = resource.Id;

                logger.LogInformation($"✅ Created resource '{definition.Key}' (Level: {level})");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"❌ Failed to process resource '{definition.Key}'");
                throw;
            }
        }

        // متد جایگزین: استفاده از الگوریتم BFS برای پردازش درخت
        public static async Task SeedResourcesBFSAsync(
            AuthorizationDbContext dbContext,
            IConfiguration config,
            ILogger logger)
        {
            logger.LogInformation("🚀 Starting BFS resource seeding...");

            try
            {
                var existingResources = await GetExistingResourcesMap(dbContext);
                var rootDefinitions = GetResourceDefinitions();
                var queue = new Queue<(ResourceDefinition Definition, Guid? ParentId)>();

                // اول تمام ریشه‌ها را به صف اضافه می‌کنیم
                foreach (var root in rootDefinitions)
                {
                    queue.Enqueue((root, null));
                }

                while (queue.Count > 0)
                {
                    var (definition, parentId) = queue.Dequeue();

                    // پردازش Resource فعلی
                    if (!existingResources.ContainsKey(definition.Key))
                    {
                        var resource = new Resource(
                            definition.Key,
                            definition.Name,
                            definition.Type.ToEnumOrDefault(ResourceType.Ui),
                            definition.Category.ToEnumOrDefault(ResourceCategory.System),
                            parentId,
                            definition.Description,
                            definition.Order,
                            definition.Icon,
                            definition.Path
                        );

                        await dbContext.Set<Resource>().AddAsync(resource);
                        await dbContext.SaveChangesAsync(); // Save برای گرفتن Id

                        existingResources[definition.Key] = resource;
                        logger.LogInformation($"✅ Created resource '{definition.Key}'");

                        // برای پردازش بهتر، Id جدید را برای والد شدن استفاده می‌کنیم
                        parentId = resource.Id;
                    }
                    else
                    {
                        // اگر Resource از قبل وجود دارد، Id آن را برای فرزندان استفاده می‌کنیم
                        parentId = existingResources[definition.Key].Id;
                        logger.LogDebug($"ℹ️ Resource '{definition.Key}' already exists");
                    }

                    // اضافه کردن فرزندان به صف
                    foreach (var child in definition.Children)
                    {
                        queue.Enqueue((child, parentId));
                    }
                }

                logger.LogInformation("✅ BFS resource seeding completed!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during BFS resource seeding");
                throw;
            }
        }


        
        // لیست پرمیژن‌های پیش‌فرض برای نقش Admin
        private static List<PermissionDefinition> GetAdminPermissionDefinitions()
        {
            return new List<PermissionDefinition>
        {
            new()
            {
                ResourceKey = "authorization.resource", // فرض می‌کنیم این کلید وجود دارد
                Action = "Full",
                Scope = "All",
                Type = "allow",
                Description = "Full access to all authorization resources"
            }
            //,
            //new()
            //{
            //    ResourceKey = "audit.auditlog", // فرض می‌کنیم این کلید وجود دارد
            //    Action = "Full",
            //    Scope = "All",
            //    Type = "allow",
            //    Description = "Full access to audit logs"
            //}
            // می‌توانید پرمیژن‌های بیشتری اضافه کنید
        };
        }

        // متد اصلی برای ایجاد پرمیژن‌ها
        public static async Task SeedPermissionsAsync(
            AuthorizationDbContext dbContext,
            IRolePublicService roleService,
            IUserPublicService userService,
            ILogger logger)
        {
            logger.LogInformation("🚀 Starting permission seeding for admin role...");

            try
            {
                // 1. دریافت RoleId نقش Admin
                var adminRoleId = await roleService.GetAdminRoleIdAsync();
                var initializerUserId = await userService.GetUserId("initializer");
                logger.LogInformation($"Admin Role ID: {adminRoleId}");

                // 2. دریافت تعاریف پرمیژن‌ها
                var PermissionDefinitions = GetAdminPermissionDefinitions();

                // 3. دریافت کلیدهای مورد نیاز
                var resourceKeys = PermissionDefinitions
                    .Select(p => p.ResourceKey)
                    .Distinct()
                    .ToList();

                // 4. دریافت ResourceIdها از دیتابیس
                var resources = await dbContext.Set<Resource>()
                    .Where(r => resourceKeys.Contains(r.Key))
                    .ToDictionaryAsync(r => r.Key, r => r.Id);

                // 5. بررسی وجود Resourceها
                var missingResources = resourceKeys
                    .Where(key => !resources.ContainsKey(key))
                    .ToList();

                if (missingResources.Any())
                {
                    logger.LogWarning($"⚠️ The following resources were not found: {string.Join(", ", missingResources)}");
                    // می‌توانید این خطا را throw کنید یا لاگ بگیرید و ادامه دهید
                    throw new InvalidOperationException(
                        $"Missing resources: {string.Join(", ", missingResources)}. " +
                        "Make sure to seed resources first.");
                }

                // 6. ایجاد پرمیژن‌ها
                var permissionsCreated = 0;
                foreach (var definition in PermissionDefinitions)
                {
                    var resourceId = resources[definition.ResourceKey];

                    // بررسی وجود پرمیژن تکراری
                    var existingPermission = await dbContext.Set<Permission>()
                        .FirstOrDefaultAsync(p =>
                            p.AssigneeType == AssigneeType.Role &&
                            p.AssigneeId == adminRoleId &&
                            p.ResourceId == resourceId &&
                            p.Action == definition.Action.ToEnumOrDefault(PermissionAction.View) &&
                            p.Scope == definition.Scope.ToEnumOrDefault(ScopeType.Self) &&
                            p.Type == definition.Type.ToEnumOrDefault(PermissionType.allow));

                    if (existingPermission != null)
                    {
                        logger.LogDebug($"ℹ️ Permission already exists for resource '{definition.ResourceKey}', skipping...");
                        continue;
                    }

                    // ایجاد پرمیژن جدید
                    var permission = new Permission(
                        resourceId: resourceId,
                        assigneeType: AssigneeType.Role,
                        assigneeId: adminRoleId,
                        action: definition.Action.ToEnumOrDefault(PermissionAction.View),
                        scope: definition.Scope.ToEnumOrDefault(ScopeType.Self),
                        specificScopeId: null, // چون ScopeType.All است
                        type: definition.Type.ToEnumOrDefault(PermissionType.allow),
                        effectiveFrom: DateTime.UtcNow,
                        expiresAt: null, // بدون انقضا
                        description: definition.Description,
                        createdBy: "system"
                    );

                    await dbContext.Set<Permission>().AddAsync(permission);
                    permissionsCreated++;

                    logger.LogInformation($"✅ Created permission for Admin on resource '{definition.ResourceKey}'");
                }

                // 7. ایجاد پرمیژن‌های initializer
                var permissionsInitializerCreated = 0;
                foreach (var definition in PermissionDefinitions)
                {
                    var resourceId = resources[definition.ResourceKey];

                    // بررسی وجود پرمیژن تکراری
                    var existingPermission = await dbContext.Set<Permission>()
                        .FirstOrDefaultAsync(p =>
                            p.AssigneeType == AssigneeType.User &&
                            p.AssigneeId == initializerUserId &&
                            p.ResourceId == resourceId &&
                            p.Action == definition.Action.ToEnumOrDefault(PermissionAction.Full) &&
                            p.Scope == definition.Scope.ToEnumOrDefault(ScopeType.All) &&
                            p.Type == definition.Type.ToEnumOrDefault(PermissionType.allow));

                    if (existingPermission != null)
                    {
                        logger.LogDebug($"ℹ️ Permission already exists for resource '{definition.ResourceKey}', skipping...");
                        continue;
                    }

                    // ایجاد پرمیژن جدید
                    var permission = new Permission(
                        resourceId: resourceId,
                        assigneeType: AssigneeType.Role,
                        assigneeId: initializerUserId,
                        action: definition.Action.ToEnumOrDefault(PermissionAction.Full),
                        scope: definition.Scope.ToEnumOrDefault(ScopeType.All),
                        specificScopeId: null, // چون ScopeType.All است
                        type: definition.Type.ToEnumOrDefault(PermissionType.allow),
                        effectiveFrom: DateTime.UtcNow,
                        expiresAt: null, // بدون انقضا
                        description: definition.Description,
                        createdBy: "system"
                    );

                    await dbContext.Set<Permission>().AddAsync(permission);
                    permissionsInitializerCreated++;

                    logger.LogInformation($"✅ Created permission for Admin on resource '{definition.ResourceKey}'");
                }

                // 8. ذخیره تغییرات
                if (permissionsCreated > 0 || permissionsInitializerCreated > 0)
                {
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation($"✅ Created {permissionsCreated} permissions for Admin role");
                }
                else
                {
                    logger.LogInformation("ℹ️ All permissions already exist, nothing to create");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error during permission seeding");
                throw;
            }
        }

        // متد یکپارچه برای seed کردن هم منابع و هم پرمیژن‌ها
        public static async Task SeedAuthorizationDataAsync(
            AuthorizationDbContext dbContext,
            IRolePublicService roleService,
            IUserPublicService userService,
            IConfiguration config,
            ILogger logger)
        {
            logger.LogInformation("🚀 Starting full authorization data seeding...");

            // 1. Seed منابع
            await SeedResourcesAsync(dbContext, config, logger);

            // 2. Seed پرمیژن‌ها
            await SeedPermissionsAsync(dbContext, roleService, userService, logger);

            logger.LogInformation("✅ Authorization data seeding completed!");
        }
    }
}
