using Authorization.Application.Commands;
using Authorization.Application.Commands.Resource;
using Authorization.Application.DTOs.Resource;
using Authorization.Domain.Entities;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Shared.DTOs.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IResourceInternalService : IResourcePublicService
    {
        // عملیات Write با منطق پیچیده
        Task<Guid> CreateResourceAsync(CreateResourceCommand command);
        Task UpdateResourceAsync(UpdateResourceCommand command);
        Task DeleteResourceAsync(Guid resourceId);
        Task<string> GetKeyById(Guid resourceId);
        Task<Resource?> GetById(Guid resourceId);
        Task<IReadOnlyList<ResourceTreeDto>> GetByTreeStructure(Guid? RootId = null);
        Task<IReadOnlyList<ResourceDto>> GetResources();
        //Task SyncResourcesWithDefinitionsAsync();

        // عملیات Read
        /*  Task<ResourceDto> GetResourceAsync(Guid resourceId);
          Task<ResourceDto> GetResourceByKeyAsync(string key);

          // منطق کسب‌وکار پیچیده  
          Task<bool> ValidateResourceHierarchyAsync(Guid resourceId, Guid? newParentId);
          Task RebuildResourceTreeAsync();

          // متدهای جدید برای ثبت خودکار منابع
          Task RegisterModuleResourcesAsync(string moduleKey);
          Task RegisterAllModulesResourcesAsync();*/

        // متد کمکی برای کار با ResourceDefinition
        //Task<Guid> CreateOrUpdateResourceFromDefinitionAsync(ResourceDefinition definition);

    }
}
