using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Authorization
{
    public interface IResourcePublicService
    {
        /// <summary>
        /// همگام‌سازی لیست منابع یک ماژول با سیستم Authorization
        /// </summary>
        Task SyncModuleResourcesAsync(List<ResourceDefinition> resources, CancellationToken cancellationToken = default);
    }
}
