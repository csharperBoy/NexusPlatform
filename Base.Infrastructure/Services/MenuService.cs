using Base.Application.Interfaces.Processor;
using Base.Application.Interfaces.Service;
using Base.Domain.Entities;
using Base.Domain.Specifications;
using Base.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Context;
using Core.Infrastructure.Repositories;
using Core.Shared.DTOs.Authorization;
using Core.Shared.DTOs.Base;
using Core.Shared.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Services
{
    public class MenuService : IMenuInternalService
    {
        private readonly IRepository<BaseDbContext, Menu, Guid> _menuRepository;
        private readonly ISpecificationRepository<Menu, Guid> _menuSpecRepository;
        private readonly IUnitOfWork<BaseDbContext> _uow;
        private readonly ILogger<MenuService> _logger;
        private readonly UserDataContext _currentUser;
        private readonly ICachePublicService _cache;
        private readonly IMenuProcessor _menuProcessor;
        private readonly string baseCacheKey = "base:menu";

        public MenuService(
            IRepository<BaseDbContext, Menu, Guid> menuRepository,
            ISpecificationRepository<Menu, Guid> menuSpecRepository,
        IUnitOfWork<BaseDbContext> uow,
            ILogger<MenuService> logger,
            UserDataContext currentUser,
            ICachePublicService cache,
            IMenuProcessor menuProcessor)
        {
            _menuRepository = menuRepository;
            _menuSpecRepository = menuSpecRepository;
            _uow = uow;
            _logger = logger;
            _currentUser = currentUser;
            _cache = cache;
            _menuProcessor = menuProcessor;
        }
        public async Task SyncModuleMenusAsync(List<MenuDto> menus, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("🔄 Starting sync for {Count} root menus...", menus.Count);

            // 1. تبدیل درخت به لیست خطی (با رعایت ترتیب والد -> فرزند)
            var flatMenus = _menuProcessor.Flatten(menus);

            // 2. دیکشنری برای نگهداری Key -> Id (برای ست کردن ParentId بدون کوئری اضافه)
            var keyToIdMap = new Dictionary<string, Guid>();

            // 3. دریافت لیست منابع موجود برای تشخیص تکراری‌ها (بهینه‌سازی)
            // بهتر است تمام منابع این ماژول را یکجا بگیریم، اما فعلا با Key چک می‌کنیم
            // اگر تعداد زیاد است، می‌توان همه را Fetch کرد.

            foreach (var def in flatMenus)
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
                            var parentRes = await GetMenuEntityByKeyAsync(def.ParentKey);
                            if (parentRes != null)
                                parentId = parentRes.Id;
                            else _logger.LogWarning("⚠️ Parent '{ParentKey}' not found for '{Key}'", def.ParentKey, def.Key);
                        }
                    }

                    // ب) بررسی وجود Menu
                    var existingMenu = await GetMenuEntityByKeyAsync(def.Key);

                    if (existingMenu == null)
                    {
                        // --- CREATE ---
                        var newMenu = new Menu(
                            def.Title,
                            def.Key,
                            def.Description,
                            def.Path,
                            def.Icon.ToEnumOrDefault(Core.Shared.Enums.Base.Icon.Default),
                            def.Order,
                            parentId
                        );
                      
                        await _menuRepository.AddAsync(newMenu);
                        keyToIdMap[def.Key] = newMenu.Id; // نگهداری ID برای فرزندان احتمالی
                        _logger.LogDebug("➕ Added menu: {Key}", def.Key);
                    }
                    else
                    {
                        // --- UPDATE ---
                        // فقط در صورتی آپدیت می‌کنیم که تغییری کرده باشد
                        bool hasChanges = existingMenu.Title != def.Title ||
                                          existingMenu.ParentId != parentId ||
                                          existingMenu.Path != def.Path ||
                                          existingMenu.Icon != def.Icon.ToEnumOrDefault(Core.Shared.Enums.Base.Icon.Default) || 
                                          existingMenu.Order != def.Order || 
                                          existingMenu.Key != def.Key || 
                                          existingMenu.Description != def.Description 
                                          ;
                        // و سایر فیلدها...

                        if (hasChanges)
                        {
                            existingMenu.Update(
                                def.Title,
                                def.Description,
                                def.Icon.ToEnumOrDefault(Core.Shared.Enums.Base.Icon.Default),
                                def.Order,
                                def.Key
                            );
                            // اگر والد تغییر کرده
                            if (existingMenu.ParentId != parentId)
                            {
                                existingMenu.ChangeParent(parentId);
                            }
                           

                            await _menuRepository.UpdateAsync(existingMenu);
                            _logger.LogDebug("✏️ Updated menu: {Key}", def.Key);
                        }

                        keyToIdMap[def.Key] = existingMenu.Id;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to sync menu '{Key}'", def.Key);
                    throw;
                }
            }

            // 4. ذخیره نهایی
            await _uow.SaveChangesAsync(cancellationToken);

            // 5. پاکسازی کش
            await InvalidateCachesAsync();

            _logger.LogInformation("✅ Menu sync completed successfully.");
        }
        private async Task<Menu?> GetMenuEntityByKeyAsync(string key)
        {
            //var lst = await  _menuRepository.AsNoTrackingQueryable();
            //var all = lst.ToList();
            //var ent = lst.Where(l=>l.Key == key).ToList();
            var spec = new MenuByKeySpec(key);
            return await _menuSpecRepository.GetBySpecAsync(spec);

        }
        public async Task<IReadOnlyList<MenuDto>> GetByTreeStructure(Guid? RootId = null)
        {
            var cacheKey = $"{baseCacheKey}:tree:full";

            try
            {
                var cached = await _cache.GetAsync<IReadOnlyList<MenuDto>>(cacheKey);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for full resource tree");
                    return cached;
                }

                //var allResourcesSpec = new ResourceByCategorySpec();
                var allResources = await _menuRepository.GetAllAsync();

                var tree = _menuProcessor.BuildTree(allResources, RootId);
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
        public async  Task<IReadOnlyList<MenuDto>> GetMenus(string? title)
        {
            var menus = await _menuRepository.GetAllAsync();
            return menus.Select(x => new MenuDto
            {
                Title = x.Title,
                Description = x.Description,
                Icon = x.Icon.GetIconString(),
                Id = x.Id,
                Order = x.Order,
                Path = x.Path,
                
            }).ToList();
        }

        private async Task InvalidateCachesAsync()
        {
            await _cache.RemoveByPatternAsync($"{baseCacheKey}:*");
        }
    }
}
