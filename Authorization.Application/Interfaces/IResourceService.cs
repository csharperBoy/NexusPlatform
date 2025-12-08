using Authorization.Application.Commands;
using Authorization.Application.Commands.Resource;
using Authorization.Application.DTOs.Resource;
using Core.Application.Abstractions.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IResourceService
    {
        // عملیات Write با منطق پیچیده
        Task<Guid> CreateResourceAsync(CreateResourceCommand command);
        Task UpdateResourceAsync(UpdateResourceCommand command);
        Task DeleteResourceAsync(Guid resourceId);

        // عملیات Read
        Task<ResourceDto> GetResourceAsync(Guid resourceId);
        Task<ResourceDto> GetResourceByKeyAsync(string key);

        // منطق کسب‌وکار پیچیده  
        Task<bool> ValidateResourceHierarchyAsync(Guid resourceId, Guid? newParentId);
        Task RebuildResourceTreeAsync();

        // متدهای جدید برای ثبت خودکار منابع
        Task RegisterModuleResourcesAsync(string moduleKey);
        Task RegisterAllModulesResourcesAsync();
        Task SyncResourcesWithDefinitionsAsync();

        // متد کمکی برای کار با ResourceDefinition
        Task<Guid> CreateOrUpdateResourceFromDefinitionAsync(ResourceDefinition definition);

    }
}
